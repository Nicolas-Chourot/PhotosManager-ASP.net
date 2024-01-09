using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace PhotosManager.Models
{
    ///////////////////////////////////////////////////////////////
    // Ce patron de classe permet de stocker dans un fichier JSON
    // une collection d'objects. Ces derniers doivent posséder
    // la propriété int Id {get; set;}
    // Après l'instanciation il faut invoquer la méthode Init
    // pour fournir le chemin d'accès du fichier JSON.
    //
    // Tous les membres annotés avec [asset] seront traités
    // en tant que données hors BD
    //
    // Voir dans global.asax
    //
    // Author : Nicolas Chourot
    // date: Janvier 2024
    ///////////////////////////////////////////////////////////////
    public class Repository<T>
    {
        #region "Méthodes et propritées privées"
        // Pour indiquer si une transaction est en cours
        static bool TransactionOnGoing = false;
        // Pour la gestion d'imbrications de transactions
        static int  NestedTransactionsCount = 0;
        // utilisé pour prévenir des conflits entre processus
        static private readonly Mutex mutex = new Mutex();
        // cache des données du fichier JSON
        private List<T> dataList;
        // chemin d'accès absolue du fichier JSON
        private string FilePath;
        // Numéro de serie des données
        private string _SerialNumber;
        // retourne la valeur de l'attribut attributeName de l'intance data de classe T

        private object GetAttributeValue(T data, string attributeName)
        {
            return data.GetType().GetProperty(attributeName).GetValue(data, null);
        }
        // affecter la valeur de l'attribut attributeName de l'intance data de classe T
        private void SetAttributeValue(T data, string attributeName, object value)
        {
            data.GetType().GetProperty(attributeName).SetValue(data, value, null);
        }
        // Vérifier si l'attribut attributeName est présent dans la classe T
        private bool AttributeNameExist(string attributeName)
        {
            var instance = Activator.CreateInstance(typeof(T));
            var type = instance.GetType();
            var pro = type.GetProperty(attributeName);
            return (instance.GetType().GetProperty(attributeName) != null);
        }
        private bool IsBase64Value(string value)
        {
            bool isBase64 = value.Contains("data:") && value.Contains(";base64,");
            return isBase64;
        }
        private void DeleteAssets(T data)
        {
            var type = data.GetType();
            foreach (var property in type.GetProperties())
            {
                var attribute = property.GetCustomAttribute(typeof(AssetAttribute));
                if (attribute != null)
                {
                    string propName = property.Name;
                    string value = GetAttributeValue(data, propName).ToString();
                    string assetToDeletePath = HostingEnvironment.MapPath(value).ToString();
                    File.Delete(assetToDeletePath);
                }
            }
        }
        private void HandleAssetMembers(T data)
        {
            var type = data.GetType();
            foreach (var property in type.GetProperties())
            {
                var attribute = property.GetCustomAttribute(typeof(AssetAttribute));
                if (attribute != null)
                {
                    string assetsFolder = ((AssetAttribute)attribute).Folder();
                    string propName = property.Name;
                    var value = GetAttributeValue(data, propName);
                    string propValue = value != null ? value.ToString() : "";
                    T previousStoredData = Get(Id(data));
                    if (propValue == "")
                    {
                        if (previousStoredData != null)
                        {
                            SetAttributeValue(data, propName, GetAttributeValue(previousStoredData, propName).ToString());
                        }
                    }
                    else
                    {
                        if (IsBase64Value(propValue))
                        {
                            if (previousStoredData != null)
                            {
                                var previousPropValue = GetAttributeValue(previousStoredData, propName);
                                if (previousPropValue != null)
                                {
                                    string assetToDeletePath = HostingEnvironment.MapPath(previousPropValue.ToString());
                                    if (File.Exists(assetToDeletePath)) File.Delete(assetToDeletePath);
                                }
                            }
                            string newAssetServerPath;
                            string[] base64Data = propValue.Split(',');
                            string extension = base64Data[0].Replace(";base64", "").Split('/')[1];
                            // Mime patch IIS : does not support webp and avif mimes
                            if (extension.ToLower() == "webp") extension = "png";
                            if (extension.ToLower() == "avif") extension = "png";
                            string assetData = base64Data[1];
                            string assetUrl;
                            do
                            {
                                var key = Guid.NewGuid().ToString();
                                assetUrl = assetsFolder + key + "." + extension;
                                newAssetServerPath = HostingEnvironment.MapPath(assetUrl);
                                // make sure new file does not already exists 
                            } while (File.Exists(newAssetServerPath));
                            SetAttributeValue(data, propName, assetUrl);
                            var stream = new MemoryStream(Convert.FromBase64String(assetData));
                            FileStream file = new FileStream(newAssetServerPath, FileMode.Create, FileAccess.Write);
                            stream.WriteTo(file);
                            file.Close();
                            stream.Close();
                        }
                    }
                }
            }
        }
        // retourne la valeur de l'attribut Id d'une instance de classe T
        private int Id(T data)
        {
            return (int)GetAttributeValue(data, "Id");
        }
        // Lecture du fichier JSON et conservation des données dans la cache dataList
        private void ReadFile()
        {
            MarkHasChanged();
            if (dataList != null)
            {
                dataList.Clear();
            }
            try
            {
                using (StreamReader sr = new StreamReader(FilePath))
                {
                    dataList = JsonConvert.DeserializeObject<List<T>>(sr.ReadToEnd());
                }
                if (dataList == null)
                {
                    dataList = new List<T>();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        // Mise à jour du fichier JSON avec les données présentes dans la cache dataList
        private void UpdateFile()
        {
            if (dataList != null)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(dataList));
                    }
                    ReadFile();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        // retourne le prochain Id disponible
        private int NextId()
        {
            int maxId = 0;
            if (dataList == null)
                return 1;
            foreach (var data in dataList)
            {
                int Id = this.Id(data);
                if (Id > maxId)
                    maxId = Id;
            }
            return maxId + 1;
        }
        #endregion

        #region "Méthodes publiques"
        // constructeur
        public Repository()
        {
            dataList = new List<T>();
            try
            {
                // s'assurer que la propriété int Id {get; set;} est belle et bien dans la classe T
                var idExist = AttributeNameExist("Id");
                if (!idExist)
                    throw new Exception("The class Repository cannot work with types that does not contain an attribute named Id of type int.");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public bool HasChanged
        {
            get
            {
                string key = this.GetType().Name;
                if (((string)HttpContext.Current.Session[key] != _SerialNumber))
                {
                    HttpContext.Current.Session[key] = _SerialNumber;
                    return true;
                }
                return false;
            }
        }
        public void BeginTransaction()
        {
            if (!TransactionOnGoing) // todo check if nested transactions still work
            {
                mutex.WaitOne();
                TransactionOnGoing = true;
            }
            else
            {
                NestedTransactionsCount++;
            }
        }
        public void EndTransaction()
        {
            if (NestedTransactionsCount <= 0)
            {
                TransactionOnGoing = false;
                mutex.ReleaseMutex();
            }
            else
            {
                if (NestedTransactionsCount > 0)
                    NestedTransactionsCount--;
            }
        }
        // Init : reçoit le chemin d'accès absolue du fichier JSON
        // Cette méthode doit avoir été appelée avant tout
        public virtual void Init(string filePath)
        {
            if (!TransactionOnGoing) mutex.WaitOne();
            try
            {
                FilePath = filePath;
                if (string.IsNullOrEmpty(FilePath))
                {
                    throw new Exception("FilePath not set exception");
                }
                if (!File.Exists(FilePath))
                {
                    using (StreamWriter sw = File.CreateText(FilePath))
                    {
                        sw.Close();
                    };
                }
                ReadFile();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!TransactionOnGoing) mutex.ReleaseMutex();
            }
        }
        public virtual void MarkHasChanged()
        {
            _SerialNumber = Guid.NewGuid().ToString();
        }

        // Méthodes CRUD
        // Read
        public T Get(int Id)
        {
            foreach (var data in dataList)
            {
                int dataId = this.Id(data);
                if (dataId == Id)
                    return data;
            }
            return default;
        }
        public List<T> ToList()
        {
            return dataList;
        }
        // Create
        public virtual int Add(T data)
        {
            int newId = 0;
            if (!TransactionOnGoing) mutex.WaitOne(); // attendre la conclusion d'un appel concurrant
            try
            {
                newId = NextId();
                SetAttributeValue(data, "Id", newId);
                HandleAssetMembers(data);
                dataList.Add(data);
                UpdateFile();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!TransactionOnGoing) mutex.ReleaseMutex();
            }
            return newId;
        }
        // Update
        public virtual bool Update(T data)
        {
            bool success = false;
            if (!TransactionOnGoing)
                mutex.WaitOne();
            try
            {
                T dataToUpdate = Get(Id(data));
                if (dataToUpdate != null)
                {
                    int index = dataList.IndexOf(dataToUpdate);
                    HandleAssetMembers(data);
                    dataList[index] = data;
                    UpdateFile();
                    success = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!TransactionOnGoing)
                    mutex.ReleaseMutex();
            }
            return success;
        }


        // Delete
        public virtual bool Delete(int Id)
        {
            bool success = false;
            if (!TransactionOnGoing)
                mutex.WaitOne();
            try
            {
                T dataToDelete = Get(Id);

                if (dataToDelete != null)
                {
                    DeleteAssets(dataToDelete);
                    int index = dataList.IndexOf(dataToDelete);
                    dataList.RemoveAt(index);
                    UpdateFile();
                    success = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!TransactionOnGoing)
                    mutex.ReleaseMutex();
            }
            return success;
        }
        #endregion
    }
}
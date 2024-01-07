function createTimeoutPopup() {
    $('body').append(`
        <div class='popup'> 
            <div class='popupContent'>
                <div>
                    <div class='popupHearder'> Attention!</div> 
                    <h4 id='popUpMessage'></h4>
                </div>
                <div onclick='closePopup(); ' class='close-btn fa fa-close'></div> 
            </div>
           
        </div> 
    `);
}
let currentTimeouID = undefined;
let initialized = false;
let timeBeforeRedirect = 5;
let timeoutCallBack = () => { window.location.replace("/Accounts/ExpiredSession"); };
let infinite = -1;
let timeLeft = infinite;
let maxStallingTime = infinite;

function initTimeout(stallingTime = infinite, callback = timeoutCallBack) {
    maxStallingTime = stallingTime;
    timeoutCallBack = callback;
    createTimeoutPopup();
    initialized = true;
    timeout();
}
function noTimeout() {
    clearTimeout(currentTimeouID);
}
function timeout() {
    startCountdown();
}
function startCountdown() {
    if (!initialized) initTimeout();
    clearTimeout(currentTimeouID);
    $(".popup").hide();
    timeLeft = maxStallingTime;
    if (timeLeft != infinite) {
        currentTimeouID = setInterval(() => {
            timeLeft = timeLeft - 1;
            if (timeLeft > 0) {
                if (timeLeft <= 10) {
                    $(".popup").show();
                    $("#popUpMessage").text("Expiration dans " + timeLeft + " secondes");
                }
            } else {
                $("#popUpMessage").text('Redirection dans ' + (timeBeforeRedirect + timeLeft) + " secondes");
                if (timeLeft <= -timeBeforeRedirect) {
                    clearTimeout(currentTimeouID);
                    closePopup();
                    timeoutCallBack();
                }
            }
        }, 1000);
    }
}
function closePopup() {
    $(".popup").hide();
    startCountdown();
} 
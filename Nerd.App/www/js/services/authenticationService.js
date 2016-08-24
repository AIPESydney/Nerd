surveycoApp.factory('authenticationService', ['$http', '$q', 'nerdSettings', function ($http, $q, nerdSettings) {
    var serviceBase = nerdSettings.apiServiceBaseUri;
    var authServiceFactory = {};

    var authData = {
        isAuth: false,
        userName: "",
        access_token:"",
        clientId: ""
    };

    //var login = function (loginData) {

    //    var data = "grant_type=password&username=" + loginData.userName + "&password=" + loginData.password;
    //    var deferred = $q.defer();

    //    $http.post(serviceBase + 'token', data, {
    //        headers: {
    //            'Content-Type': 'application/x-www-form-urlencoded',
    //            }

    //    }).success(function (response) {
    //        localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName ,clientId: response.clientId});
    //        authenticationService.isAuth = true;
    //        authenticationService.userName = loginData.userName;
    //        deferred.resolve(response);

    //    }).error(function (err, status) {
    //        logOut();
    //        deferred.reject(err);
    //    });

    //    return deferred.promise;

    //};

    var createAuthenticationData = function () {
        if(typeof(Storage) !== "undefined") { 
            var localStorageData = localStorage.getItem("authorizationData");
            if (localStorageData != null) {
                localStorageData = JSON.parse(localStorageData);
                authData.isAuth = true;
                authData.userName = localStorageData.userName;
                authData.access_token = localStorageData.accesstoken;
                authData.clientId = localStorageData.clientId;

            } else {
                if ($.cookie('authorizationData_access_token')) {
                    authData.isAuth = true;
                    authData.access_token = $.cookie('authorizationData_access_token');
                    authData.userName = $.cookie('authorizationData_userName');
                    authData.clientId = $.cookie('authorizationData_clientId');
                } else {
                    authData.isAuth = false;
                    authData.access_token = "";
                    authData.userName = "";
                    authData.clientId = "";
                }
            }
        }
        else
        {
            if ($.cookie('authorizationData_access_token')) {
                authData.isAuth = true;
                authData.access_token = $.cookie('authorizationData_access_token');
                authData.userName = $.cookie('authorizationData_userName');
                authData.clientId = $.cookie('authorizationData_clientId');
            } else {
                authData.isAuth = false;
                authData.access_token = "";
                authData.userName = "";
                authData.clientId = "";
            }
        }
    };

    authServiceFactory.createAuthenticationData = createAuthenticationData;
    authServiceFactory.authData = authData;

    return authServiceFactory;
}]);
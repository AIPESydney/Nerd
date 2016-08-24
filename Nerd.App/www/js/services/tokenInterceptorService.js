'use strict';
surveycoApp.factory('tokenInterceptorService', ['$q', '$injector', '$location', function ($q, $injector, $location) {
    var tokenInterceptorServiceFactory = {};
    var request = function (config) {
        config.headers = config.headers || {};
        var authenticationService = $injector.get('authenticationService');
        var authData = authenticationService.authData;
        if (authData) {
            if (config.url !="/signalr") {
                config.headers.Authorization = 'Bearer ' + authData.access_token;
            }
            
        }
        return config;
    }

    var responseError = function (rejection) {
        if (rejection.status === 401) {
         
        }
        //return $q.reject(rejection);
    }


    tokenInterceptorServiceFactory.request = request;
    tokenInterceptorServiceFactory.responseError = responseError;
    return tokenInterceptorServiceFactory;
}]);
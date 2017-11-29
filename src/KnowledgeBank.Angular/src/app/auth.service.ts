import { LocationStrategy } from '@angular/common/';
import { Injectable, EventEmitter } from '@angular/core';
import { Http, Headers, RequestOptions, Response, URLSearchParams, ResponseContentType } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { UserManager, Log, User } from 'oidc-client';
import { getAuthSettings } from './auth.config';
import { ProjectService } from 'app/services/project.service';
import { DownloadedFile } from './models/downloaded-file';

@Injectable()
export class AuthService {
    mgr: UserManager;
    userLoadedEvent: EventEmitter<User | null> = new EventEmitter<User | null>();
    currentUser: User | null;
    loggedIn = false;
    authHeaders: Headers;
    initUserPromise: Promise<User>;

    constructor(private http: Http,
        private location: LocationStrategy,
        private projectService: ProjectService) {
        // To turn on the logger uncomment the below line
        Log.logger = console;

        this.mgr = new UserManager(getAuthSettings(location.getBaseHref()));

        this.initUserPromise = this.mgr.getUser();
        this.initUserPromise.then((user) => {
            console.log('got user');
            this.updateAuthUserData(user);
        }).catch(function (err) {
            console.log(err);
        });

        this.mgr.events.addUserLoaded((loadedUser) => {
            console.log('user loaded');
            this.updateAuthUserData(loadedUser);
        });

        this.mgr.events.addUserUnloaded((e) => {
            console.log('user unloaded');
            this.loggedIn = false;
        });
    }

    get isAuthenticated() {
        return this.loggedIn && this.currentUser && !this.currentUser.expired;
    }

    clearState() {
        this.mgr.clearStaleState().then(function () {
            console.log('clearStateState success');
        }).catch(function (e) {
            console.log('clearStateState error', e.message);
        });
    }

    getUser() {
        this.mgr.getUser().then((user) => {
            console.log('got user');
            this.updateAuthUserData(user);
        }).catch(function (err) {
            console.log(err);
        });
    }

    removeUser() {
        this.mgr.removeUser().then(() => {
            this.updateAuthUserData(null);
            console.log('user removed');
        }).catch(function (err) {
            console.log(err);
        });
    }

    startSigninMainWindow() {
        this.mgr.signinRedirect({ data: 'some data' }).then(function () {
            console.log('signinRedirect done');
        }).catch(function (err) {
            console.log(err);
        });
    }
    endSigninMainWindow() {
        this.mgr.signinRedirectCallback().then(function (user) {
            console.log('signed in', user);
        }).catch(function (err) {
            console.log(err);
        });
    }

    startSignoutMainWindow() {
        this.mgr.signoutRedirect().then(function (resp) {
            console.log('signed out', resp);
            setTimeout(5000, () => {
                console.log('testing to see if fired...');

            });
        }).catch(function (err) {
            console.log(err);
        });
    };

    endSignoutMainWindow() {
        this.mgr.signoutRedirectCallback().then(function (resp) {
            console.log('signed out', resp);
        }).catch(function (err) {
            console.log(err);
        });
    };

    /**
     * Example of how you can make auth request using angulars http methods.
     * @param options if options are not supplied the default content type is application/json
     */
    AuthGet(url: string, options?: RequestOptions, tenant?: number): Observable<Response> {
        options = this._setRequestOptions(options, tenant);
        return this.http.get(url, options);
    }

    /**
     * @param options if options are not supplied the default content type is application/json
     */
    AuthPut(url: string, data: any, options?: RequestOptions, tenant?: number): Observable<Response> {
        options = this._setRequestOptions(options, tenant);
        // bugfix, cant make put request, because it gets overrided
        // TODO: make pull request to dev
        options.body = JSON.stringify(data);
        return this.http.put(url, data, options);
    }

    /**
     * @param options if options are not supplied the default content type is application/json
     */
    AuthDelete(url: string, options?: RequestOptions, tenant?: number): Observable<Response> {
        options = this._setRequestOptions(options, tenant);
        return this.http.delete(url, options);
    }

    /**
     * @param options if options are not supplied the default content type is application/json
     */
    AuthPost(url: string, data: any, options?: RequestOptions, tenant?: number): Observable<Response> {
        options = this._setRequestOptions(options, tenant);
        return this.http.post(url, data, options);
    }

    AuthPostWithFile(url: string, dto: any, files: File[], tenant: number, isUpdate: boolean): Observable<Response> {
      let formData: FormData = new FormData();
      for (let file of files) {
        formData.append("attachments", file, file.name);
      }
      formData.append('dto', JSON.stringify(dto));

      let options = this._setRequestOptions(undefined, tenant);
      options.headers = new Headers();
      for (let header of this.authHeaders.keys()) {
        if (header.toLowerCase() !== 'Content-Type'.toLowerCase()) {
          options.headers.append(header, this.authHeaders.get(header) || '');
        }
      }
      if (isUpdate) {
          return this.http.put(url, formData, options);
      }
      return this.http.post(url, formData, options);
    }

    //AuthPutWithFile(url: string, dto: any, files: File[], tenant: number): Observable<Response> {
    //    let formData: FormData = new FormData();
    //    for (let file of files) {
    //        formData.append("attachments", file, file.name);
    //    }
    //    formData.append('dto', JSON.stringify(dto));

    //    let options = this._setRequestOptions(undefined, tenant);
    //    options.headers = new Headers();
    //    for (let header of this.authHeaders.keys()) {
    //        if (header.toLowerCase() !== 'Content-Type'.toLowerCase()) {
    //            options.headers.append(header, this.authHeaders.get(header) || '');
    //        }
    //    }

    //    return this.http.put(url, formData, options);
    //}

    AuthDownload(url: string, tenant?: number): Observable<DownloadedFile> {
      let options = this._setRequestOptions(undefined, tenant);
      options.responseType = ResponseContentType.Blob;

      return this.http.get(url, options)
        .map(res => {
          let filename = '';
          if(res.headers) {
            let contentDisposition = res.headers.get('Content-Disposition') || '';
            let matches = contentDisposition.match(/filename=(.+);/);
            filename = matches != null && matches[1] ? matches[1] : '';
          }

          return { blob: res.blob(), filename: filename };
        });
    }

    private _setAuthHeaders(user: any) {
        this.authHeaders = new Headers();
        this.authHeaders.append('Authorization', user.token_type + ' ' + user.access_token);
        this.authHeaders.append('Content-Type', 'application/json');
    }


    private _buildSearchParams(tenant?: number) {
        const params = new URLSearchParams();
        if (this.projectService.selectedProject) {
            const t = tenant ? tenant : this.projectService.selectedProject;
            params.set('tenant', t.toString());
        }
        return params;
    }

    private _setRequestOptions(options?: RequestOptions, tenant?: number): RequestOptions {
        const searchParams = this._buildSearchParams(tenant);
        if (options) {
            if (options.headers) {
                options.headers.append(this.authHeaders.keys()[0], this.authHeaders.values()[0][0]);
            } else {
                options.headers = this.authHeaders;
            }
            if (options.params) {
              options.params.appendAll(searchParams);
            } else {
              options.params = searchParams;
            }
        } else {
            options = new RequestOptions({ headers: this.authHeaders, search: searchParams });
        }

        return options;
    }

    private updateAuthUserData(user: User | null) {
        this.loggedIn = user != null;
        this.currentUser = user;
        if (user) {
            this._setAuthHeaders(user);
        }
        this.userLoadedEvent.emit(user);
    }
}

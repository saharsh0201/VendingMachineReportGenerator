import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./components/login-page/login-page').then(m => m.LoginPageComponent)
  },
  {
    path: 'upload',
    loadComponent: () =>
      import('./components/upload-form/upload-form').then(m => m.UploadFormComponent),
    canActivate: [authGuard]
  },
  {
    path: 'history',
    loadComponent: () =>
      import('./components/history-page/history-page').then(m => m.HistoryPageComponent),
    canActivate: [authGuard]
  },
  {
    path: 'result/:id',
    loadComponent: () =>
      import('./components/result-page/result-page').then(m => m.ResultPageComponent),
    canActivate: [authGuard]
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];

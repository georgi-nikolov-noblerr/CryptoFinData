import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { CryptoDashboardComponent } from './features/dashboard/crypto-dashboard/crypto-dashboard.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'dashboard',
    component: CryptoDashboardComponent,
  },
  { 
    path: '', 
    redirectTo: '/dashboard', 
    pathMatch: 'full' 
  },
  { 
    path: '**', 
    redirectTo: '/dashboard' 
  }
];
import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { UploadImageComponent } from './UploadImage/UploadImage.component';
import { UserLayoutComponent } from './layouts/user-layout/user-layout.component';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { UserDashboardComponent } from './UserDashboard/UserDashboard.component';
import { authGuard } from './auth.guard';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },

    {
        path: '',
        component: UserLayoutComponent,
        canActivate: [authGuard],
        children: [
            { path: 'dashboard', component: UserDashboardComponent }, 
            
            { path: 'upload', component: UploadImageComponent },

            { path: '', redirectTo: 'dashboard', pathMatch: 'full' } 
        ]
    },

    {
        path: 'admin-panel',
        component: AdminLayoutComponent,
        canActivate: [authGuard],
        data: { role: 'admin' },
        children: [
            { path: 'dashboard', component: AdminDashboardComponent },
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    },

    { path: '**', redirectTo: 'login' }
];
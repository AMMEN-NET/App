import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; // Importante para *ngIf y *ngFor
import { DashboardService } from '../../proxy/estadisticas/dashboard.service';
import { DashboardResumenDto } from '../../proxy/estadisticas/models';
import { PageModule } from '@abp/ng.components/page'; // Para el layout estándar de ABP
import { CoreModule } from '@abp/ng.core';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, PageModule, CoreModule],
  templateUrl: './admin-dashboard.component.html',
})
export class AdminDashboardComponent implements OnInit {
  resumen: DashboardResumenDto | null = null;
  loading = true;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getResumen().subscribe({
      next: (data) => {
        this.resumen = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error cargando dashboard', err);
        this.loading = false;
      }
    });
  }
}
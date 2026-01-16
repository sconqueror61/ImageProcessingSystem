import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileService } from '../file.service';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [CommonModule],
  // DİKKAT: Dosya adınız UserDashboard.component.html ise burası da öyle olmalı!
  templateUrl: './UserDashboard.component.html', 
  styleUrls: ['./UserDashboard.component.css'] 
})
export class UserDashboardComponent implements OnInit {
  
  details: any[] = [];
  currentPage: number = 1;
  totalCount: number = 0;
  isLoading: boolean = false;
  pageSize: number = 10; 

  constructor(private fileService: FileService) {}

  ngOnInit() {
    this.loadData(1);
  }

  // Verileri Yükle
  loadData(page: number) {
    this.isLoading = true;
    this.fileService.getDashboardDetails(page).subscribe({
      next: (res: any) => {
        this.details = res.items; 
        this.totalCount = res.totalCount;
        this.currentPage = res.pageNumber;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Data loading error:', err);
        this.isLoading = false;
      }
    });
  }

  // Sayfalama - İleri
  nextPage() {
    if ((this.currentPage * this.pageSize) < this.totalCount) {
      this.loadData(this.currentPage + 1);
    }
  }

  // Sayfalama - Geri
  prevPage() {
    if (this.currentPage > 1) {
      this.loadData(this.currentPage - 1);
    }
  }

  // Dosya İndir
  onDownload(fileId: string, fileName: string) {
    this.fileService.downloadFile(fileId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => console.error('Download error:', err)
    });
  }

  // Dosya Sil
  onDelete(fileId: string) {
    if(confirm('Are you sure you want to delete this record and file?')) {
      this.fileService.deleteFile(fileId).subscribe({
        next: () => {
          alert('Record deleted successfully.');
          this.loadData(this.currentPage); 
        },
        error: (err) => {
          console.error(err);
          alert('Error deleting file: ' + (err.error?.message || 'Unknown error'));
        }
      });
    }
  }
}
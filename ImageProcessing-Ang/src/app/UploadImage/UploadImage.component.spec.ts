/* tslint:disable:no-unused-variable */
// 'async' yerine 'waitForAsync' kullanıyoruz
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { UploadImageComponent } from './UploadImage.component'; // Dosya adını kontrol edin

describe('UploadImageComponent', () => {
  let component: UploadImageComponent;
  let fixture: ComponentFixture<UploadImageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      // Standalone bileşenler 'declarations' yerine 'imports' dizisine eklenir
      imports: [ UploadImageComponent ],
      // API istekleri (HttpClient) için provider eklemeliyiz
      providers: [ provideHttpClient() ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UploadImageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
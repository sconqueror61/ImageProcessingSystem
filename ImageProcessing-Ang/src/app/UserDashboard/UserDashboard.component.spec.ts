import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing'; // 1. async yerine waitForAsync
import { UserDashboardComponent } from './UserDashboard.component';
import { HttpClientTestingModule } from '@angular/common/http/testing'; // 2. HTTP istekleri patlamasın diye ekledik

describe('UserDashboardComponent', () => {
  let component: UserDashboardComponent;
  let fixture: ComponentFixture<UserDashboardComponent>;

  beforeEach(waitForAsync(() => { // 3. async -> waitForAsync değişimi
    TestBed.configureTestingModule({
      // 4. Component 'standalone: true' olduğu için declarations değil imports kullanıyoruz
      imports: [ 
        UserDashboardComponent, 
        HttpClientTestingModule 
      ] 
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
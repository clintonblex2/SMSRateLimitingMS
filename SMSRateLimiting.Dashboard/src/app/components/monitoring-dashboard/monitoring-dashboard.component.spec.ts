import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { MonitoringDashboardComponent } from './monitoring-dashboard.component';
import { MonitoringService } from '../../services/monitoring.service';
import { ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { NgApexchartsModule } from 'ng-apexcharts';

describe('MonitoringDashboardComponent', () => {
  let component: MonitoringDashboardComponent;
  let fixture: ComponentFixture<MonitoringDashboardComponent>;
  let monitoringService: jasmine.SpyObj<MonitoringService>;

  const mockStats = {
    messagesPerSecond: {
      '2024-11-10T10:00:00': 5,
      '2024-11-10T10:00:01': 3
    },
    currentStats: {
      currentCount: 3,
      remainingCapacity: 7,
      maxRequests: 10,
      windowDuration: '00:00:01'
    },
    historicalStats: {
      totalRequests: 100,
      totalBlocked: 10,
      averageRequestsPerSecond: 2.5,
      peakRequestsPerSecond: 8,
      peakTime: '2024-11-10T10:00:00'
    },
    phoneNumber: null,
    timeRange: {
      startTime: '2024-11-10T09:00:00',
      endTime: '2024-11-10T10:00:00'
    }
  };

  beforeEach(async () => {
    monitoringService = jasmine.createSpyObj('MonitoringService',
      ['setTimeRange', 'setActivePhoneNumber'], {
      stats$: of(mockStats),
      phoneNumbers$: of(['+1234567890', '+9876543210'])
    }
    );

    await TestBed.configureTestingModule({
      declarations: [MonitoringDashboardComponent],
      imports: [
        ReactiveFormsModule,
        MatCardModule,
        MatFormFieldModule,
        MatSelectModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatButtonToggleModule,
        BrowserAnimationsModule,
        NgApexchartsModule
      ],
      providers: [
        { provide: MonitoringService, useValue: monitoringService }
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MonitoringDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default view and form', () => {
    const now = new Date();
    const startOfDay = new Date(now.setHours(0, 0, 0, 0));
    const endOfDay = new Date(now.setHours(23, 59, 59, 999));

    expect(component.activeView).toBe('account');
    expect(component.filterForm.get('phoneNumber')?.value).toBe('');
    expect(component.filterForm.get('startTime')?.value).toEqual(startOfDay);
    expect(component.filterForm.get('endTime')?.value).toEqual(endOfDay);
  });

  it('should handle view change', () => {
    // Switch to phone view
    component.onViewChange('phone');
    expect(component.activeView).toBe('phone');

    // Switch back to account view
    component.onViewChange('account');
    expect(component.activeView).toBe('account');
    expect(monitoringService.setActivePhoneNumber).toHaveBeenCalledWith(null);
    expect(component.filterForm.get('phoneNumber')?.value).toBe('');
  });

  it('should handle start date selection', () => {
    const startDate = new Date('2024-11-10');
    const event = { value: startDate };

    component.onStartDateSelected(event);

    const expectedDate = new Date(startDate);
    expectedDate.setHours(0, 0, 0, 0);

    expect(monitoringService.setTimeRange).toHaveBeenCalled();
  });

  it('should handle end date selection', () => {
    const endDate = new Date('2024-11-10');
    const event = { value: endDate };

    component.onEndDateSelected(event);

    const expectedDate = new Date(endDate);
    expectedDate.setHours(23, 59, 59, 999);

    expect(monitoringService.setTimeRange).toHaveBeenCalled();
  });

  it('should transform chart data correctly', () => {
    const messagesPerSecond = {
      '2024-11-10T10:00:00': 5,
      '2024-11-10T10:00:01': 3
    };

    const result = component.transformChartData(messagesPerSecond);
    expect(result.length).toBe(2);
    expect(result[0].y).toBe(5);
    expect(result[1].y).toBe(3);
    expect(result[0].x).toBeLessThan(result[1].x);
  });

  it('should clean up subscriptions on destroy', () => {
    const destroySpy = spyOn(component['destroy$'], 'next');
    const completeSpy = spyOn(component['destroy$'], 'complete');

    component.ngOnDestroy();

    expect(destroySpy).toHaveBeenCalled();
    expect(completeSpy).toHaveBeenCalled();
  });
});

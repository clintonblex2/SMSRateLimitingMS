import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MonitoringService } from './monitoring.service';
import { environment } from '../../environment';

describe('MonitoringService', () => {
  let service: MonitoringService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [MonitoringService]
    });

    service = TestBed.inject(MonitoringService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch phone numbers', () => {
    const mockPhoneNumbers = ['+1234567890', '+9876543210'];

    service.phoneNumbers$.subscribe(numbers => {
      expect(numbers).toEqual(mockPhoneNumbers);
    });

    const req = httpMock.expectOne(`${environment.baseUrl}/monitoring/phone-numbers`);
    expect(req.request.method).toBe('GET');
    req.flush(mockPhoneNumbers);
  });

  it('should handle time range updates', fakeAsync(() => {
    const startDate = new Date('2024-11-10T00:00:00');
    const endDate = new Date('2024-11-10T23:59:59.999');

    service.setTimeRange(startDate, endDate);

    service.stats$.subscribe();

    tick(0);

    const req = httpMock.expectOne(request =>
      request.url === `${environment.baseUrl}/monitoring/statistics` &&
      request.params.get('startTime') === startDate.toISOString() &&
      request.params.get('endTime') === endDate.toISOString()
    );
    expect(req.request.method).toBe('GET');
  }));

  it('should handle phone number updates', fakeAsync(() => {
    const phoneNumber = '+1234567890';

    service.setActivePhoneNumber(phoneNumber);

    service.stats$.subscribe();

    tick(0);

    const req = httpMock.expectOne(request =>
      request.url === `${environment.baseUrl}/monitoring/statistics` &&
      request.params.get('businessPhoneNumber') === phoneNumber
    );
    expect(req.request.method).toBe('GET');
  }));

  it('should clean up on destroy', () => {
    const nextSpy = spyOn(service['destroy$'], 'next');
    const completeSpy = spyOn(service['destroy$'], 'complete');

    service.ngOnDestroy();

    expect(nextSpy).toHaveBeenCalled();
    expect(completeSpy).toHaveBeenCalled();
  });
});

import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable, OnDestroy } from "@angular/core";
import { BehaviorSubject, Observable, Subject, catchError, combineLatest, of, retry, shareReplay, switchMap, takeUntil, timer } from "rxjs";
import { MonitoringStats } from "../models/monitoring.models";
import { environment } from "../../environment";

@Injectable({
  providedIn: 'root'
})
export class MonitoringService implements OnDestroy {
  private baseUrl = `${environment.baseUrl}`;
  private refreshInterval = 3000; // 3 secs
  private destroy$ = new Subject<void>();

  private activePhoneNumberSubject = new BehaviorSubject<string | null>(null);
  private timeRangeSubject = new BehaviorSubject<{ start: Date; end: Date }>({
    start: new Date(new Date().setHours(0, 0, 0, 0)),
    end: new Date(new Date().setHours(23, 59, 59, 999))
  });

  // Expose observables
  activePhoneNumbers$ = this.activePhoneNumberSubject.asObservable();
  timeRange$ = this.timeRangeSubject.asObservable();

  // Real-time stats stream
  stats$ = combineLatest([
    timer(0, this.refreshInterval),
    this.timeRangeSubject,
    this.activePhoneNumberSubject
  ]).pipe(
    takeUntil(this.destroy$),
    switchMap(([_, timeRange, phoneNumber]) => {
      let params = new HttpParams()
        .set('startTime', timeRange.start.toISOString())
        .set('endTime', timeRange.end.toISOString());

      if (phoneNumber) {
        params = params.set('businessPhoneNumber', phoneNumber);
      }

      return this.http.get<MonitoringStats>(`${this.baseUrl}/monitoring/statistics`, { params });
    }),
    retry({ count: 3, delay: 1000 }),
    shareReplay(1),
    catchError(error => {
      return of(null);
    })
  );

  // Real-time phone numbers stream
  phoneNumbers$ = timer(0, this.refreshInterval).pipe(
    takeUntil(this.destroy$),
    switchMap(() => this.getPhoneNumbers()),
    retry({ count: 3, delay: 1000 }),
    shareReplay(1)
  );

  constructor(private http: HttpClient) { }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  setActivePhoneNumber(phoneNumber: string | null) {
    this.activePhoneNumberSubject.next(phoneNumber);
  }

  setTimeRange(start: Date, end: Date) {
    this.timeRangeSubject.next({ start, end });
  }

  getPhoneNumbers(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/monitoring/phone-numbers`)
      .pipe(
        catchError(error => {
          throw error;
        })
      )
  }
}

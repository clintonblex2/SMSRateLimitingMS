import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MonitoringService } from '../../services/monitoring.service';
import { map, catchError, debounceTime, takeUntil } from 'rxjs/operators';
import { Subject, of } from 'rxjs';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexYAxis,
  ApexTooltip,
  ApexStroke,
  ApexFill,
  ApexDataLabels,
  ApexGrid,
  ApexStates,
  ApexMarkers,
  ApexTheme
} from 'ng-apexcharts';

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  yaxis: ApexYAxis;
  tooltip: ApexTooltip;
  stroke: ApexStroke;
  fill: ApexFill;
  dataLabels: ApexDataLabels,
  grid: ApexGrid;
  states: ApexStates;
  markers: ApexMarkers;
  theme: ApexTheme;
};

@Component({
  selector: 'app-monitoring-dashboard',
  templateUrl: './monitoring-dashboard.component.html',
  styleUrls: ['./monitoring-dashboard.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None
})
export class MonitoringDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  readonly viewOptions = [
    { value: 'account', label: 'Account Overview' },
    { value: 'phone', label: 'Phone Number Details' }
  ];

  activeView: string = 'account';
  filterForm!: FormGroup;
  isLoading: boolean = false;

  stats$ = this.monitoringService.stats$.pipe(
    takeUntil(this.destroy$)
  );

  phoneNumbers$ = this.monitoringService.phoneNumbers$.pipe(
    takeUntil(this.destroy$)
  );

  chartOptions: Partial<ChartOptions> = {
    series: [],
    chart: {
      type: 'area',
      height: 350,
      background: '#1a2234',
      foreColor: '#a3aed0',
      toolbar: {
        show: false
      },
      zoom: {
        enabled: false
      },
      animations: {
        enabled: true,
        easing: 'easeinout',
        speed: 800,
        dynamicAnimation: {
          enabled: true,
          speed: 350
        }
      }
    },
    stroke: {
      curve: 'smooth',
      width: 4,
      lineCap: 'round',
      colors: ['#7b8fff']
    },
    fill: {
      type: 'gradient',
      gradient: {
        shadeIntensity: 1,
        opacityFrom: 0.7,
        opacityTo: 0.1,
        stops: [0, 90, 100],
        colorStops: [
          {
            offset: 0,
            color: '#7b8fff',
            opacity: 0.3
          },
          {
            offset: 100,
            color: 'rgba(123, 143, 255, 0.1)',
            opacity: 0.1
          }
        ]
      }
    },
    dataLabels: {
      enabled: false
    },
    grid: {
      borderColor: '#2d3546',
      strokeDashArray: 5,
      xaxis: {
        lines: {
          show: true
        }
      },
      yaxis: {
        lines: {
          show: true
        }
      },
      padding: {
        top: 0,
        right: 0,
        bottom: 0,
        left: 0
      }
    },
    tooltip: {
      theme: 'dark',
      x: {
        format: 'HH:mm:ss'
      },
      y: {
        formatter: (value: number) => `${value} msg / sec`
      },
      style: {
        fontSize: '12px',
        fontFamily: 'Inter, sans-serif'
      }
    },
    xaxis: {
      type: 'datetime',
      labels: {
        datetimeUTC: false,
        format: 'HH:mm:ss',
        style: {
          colors: '#a3aed0',
          fontSize: '12px',
          fontWeight: 500
        }
      },
      axisBorder: {
        show: false
      },
      axisTicks: {
        show: false
      }
    },
    yaxis: {
      title: {
        text: 'Messages/Second',
        style: {
          fontSize: '12px',
          fontWeight: 500,
          color: '#a3aed0'
        }
      },
      min: 0,
      labels: {
        formatter: (value) => value.toFixed(0),
        style: {
          colors: '#a3aed0',
          fontSize: '12px',
          fontWeight: 500
        }
      }
    },
    states: {
      hover: {
        filter: {
          type: 'lighten',
          value: 0.1
        }
      },
      active: {
        filter: {
          type: 'darken',
          value: 0.1
        }
      }
    },
    markers: {
      size: 5,
      colors: ['#4318FF'],
      strokeColors: '#1a2234',
      strokeWidth: 2,
      hover: {
        size: 7,
        sizeOffset: 3
      }
    },
    theme: {
      mode: 'dark'
    }
  };

  constructor(
    private monitoringService: MonitoringService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    // Subscribe to individual form control changes
    this.filterForm.get('startTime')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(value => {
        this.updateTimeRange();
      });

    this.filterForm.get('endTime')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(value => {
        this.updateTimeRange();
      });

    this.filterForm.get('phoneNumber')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(value => {
        if (this.activeView === 'phone') {
          this.monitoringService.setActivePhoneNumber(value);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateTimeRange(start?: Date, end?: Date): void {
    if (start && end) {
      if (start <= end) {
        // Update the time range in service
        this.monitoringService.setTimeRange(start, end);
        this.filterForm.setErrors(null);
      } else {
        this.filterForm.setErrors({ invalidRange: true });
      }
    }
  }

  onStartDateSelected(event: any): void {
    const startDate = event.value;
    if (startDate) {
      // Set time to start of the selected day
      const start = new Date(startDate);
      start.setHours(0, 0, 0, 0);
      this.updateTimeRange(start, this.filterForm.get('endTime')?.value);
    }
  }

  onEndDateSelected(event: any): void {
    const endDate = event.value;
    if (endDate) {
      // Set time to end of the selected day
      const end = new Date(endDate);
      end.setHours(23, 59, 59, 999);
      this.updateTimeRange(end, this.filterForm.get('startTime')?.value);
    }
  }

  private initializeForm(): void {
    const now = new Date();
    const startOfToday = new Date(now.setHours(0, 0, 0, 0));
    const endOfToday = new Date(now.setHours(23, 59, 59, 999));

    this.filterForm = this.fb.group({
      phoneNumber: [''],
      startTime: [startOfToday, Validators.required],
      endTime: [endOfToday, Validators.required]
    });
  }

  onViewChange(view: string): void {
    this.activeView = view;
    if (view === 'account') {
      this.monitoringService.setActivePhoneNumber(null);
      this.filterForm.patchValue({ phoneNumber: '' });
    }
  }

  transformChartData(messagesPerSecond: { [key: string]: number }): any[] {
    return Object.entries(messagesPerSecond)
      .map(([timestamp, count]) => ({
        x: new Date(timestamp).getTime(),
        y: count
      }))
      .sort((a, b) => a.x - b.x);
  }
}

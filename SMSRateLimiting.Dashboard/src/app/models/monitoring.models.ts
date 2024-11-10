export interface CounterStats {
  currentCount: number;
  remainingCapacity: number;
  maximumRequests: number; 
  windowDuration: string;
  utilizationPercentage: number;
}

export interface HistoricalStats {
  totalRequests: number;
  totalBlocked: number;
  averageRequestsPerSecond: number;
  peakRequestsPerSecond: number;
  peakTime?: string;
  blockedPercentage: number;
}

export interface TimeRange {
  startTime: string;
  endTime: string;
  duration: string;
}

export interface MonitoringStats {
  messagesPerSecond: { [key: string]: number };
  currentStats: CounterStats;
  historicalStats: HistoricalStats;
  phoneNumber?: string;
  timeRange: TimeRange;
}

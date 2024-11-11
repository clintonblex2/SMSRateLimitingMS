# SMS Rate Limiting System

A microservice-based solution for managing SMS message rate limits across business phone numbers and account-wide quotas. This system helps prevent costly API calls by implementing a real-time rate-limiting strategy.

## Table of Contents
- [Overview](#overview)
- [System Architecture](#system-architecture)
- [Features](#features)
- [Technical Stack](#technical-stack)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Monitoring Dashboard](#monitoring-dashboard)
- [Testing](#testing)
- [Performance](#performance)

## Overview

This system serves as a gatekeeper for SMS message sending operations, ensuring compliance with provider-imposed rate limits:
- Per-phone number message limits per second
- Account-wide message limits per second

### Key Benefits
- Prevents unnecessary API calls to the SMS provider
- Reduces operational costs
- Provides real-time monitoring
- Ensures efficient resource management

## System Architecture

The solution consists of three main components:
1. .NET Core Rate Limiting Service
2. Angular Monitoring Dashboard
3. In-Memory Counter System

### Rate Limiting Service
- Built with .NET Core 8.0
- RESTful API endpoints
- Sliding window rate limiting algorithm
- Automatic resource cleanup for inactive numbers

### Monitoring Dashboard
- Real-time statistics visualization
- Filtering capabilities
- Responsive design

![image](https://github.com/user-attachments/assets/d349338a-b708-464f-a9ed-007e0311c021)

## Features

### Core Features
- Real-time rate limit checking
- Automatic resource cleanup
- High-performance counter implementation
- RESTful API endpoints

### Monitoring Features
- Real-time message rate visualization
- Per-number statistics
- Account-wide metrics
- Custom date range filtering
- Phone number filtering

## Technical Stack

### Backend
- .NET Core 8.0
- C#
- Swagger/OpenAPI
- xUnit for testing

### Frontend
- Angular 18
- RxJS
- NgRx (state management)
- Angular Material
- Chart.js for visualizations

## Getting Started

### Prerequisites
```bash
- .NET Core SDK 8.0
- Node.js
- Angular CLI
```

### Installation

1. Clone the repository
```bash
git clone [repository-url]
```

2. Backend Setup
```bash
cd RateLimitingService
dotnet restore
dotnet run
```

3. Frontend Setup
```bash
cd monitoring-dashboard
npm install
ng serve
```

## API Documentation

### Rate Limiting Endpoints

![image](https://github.com/user-attachments/assets/05a206f3-14cf-4930-907a-ad1e49e794b5)

#### Check Message Rate Limit
```http
POST /api/smsratelimit/check
```

Request Body:
```json
{
  "businessPhoneNumber": "+1234567890"
}
```

Response:
```json
{
  "canSendSMS": true,
  "reasonForDenial": "string"
}
```

## Monitoring Dashboard

### Account-Wide Monitoring
![image](https://github.com/user-attachments/assets/15f29765-eda4-4349-bd10-34c51cf63f96)

Features:
- Real-time message rate graph
- Current capacity utilization
- Historical trends
- Peak usage indicators

### Per-Number Monitoring

![image](https://github.com/user-attachments/assets/26c12dbe-f401-482a-9185-681be8c09986)

Features:
- Individual phone number statistics
- Message rate trends
- Capacity warnings
- Historical data

### Filtering Options

Capabilities:
- Date range selection
- Phone number search
- Custom time windows
- Export functionality

## Testing

### Unit Tests
```bash
dotnet test
```
![image](https://github.com/user-attachments/assets/35bb1761-3171-40cf-a7c2-a66f154852f2)


Performance metrics:
- Response time under load
- Concurrent request handling
- Resource utilization
- Error rates

## Resource Management

The system implements automatic cleanup of inactive phone numbers:
- Configurable timeout period
- Memory optimization
- Automatic resource reclamation
- Monitoring of resource usage

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---


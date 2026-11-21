# 📈 Financial Market Monitor - Nepal Stock & Gold/Silver

A multi-threaded financial market monitoring system built using C# .NET WinForms.

The application collects, monitors, and visualizes Nepal Stock Exchange (NEPSE) market information along with Gold and Silver prices through independent collection threads, providing a real-time dashboard experience similar to professional financial monitoring systems.

---

## 🚀 Features

### Market Dashboard

✅ NEPSE Index Monitoring

✅ Gold Price Tracking

✅ Silver Price Tracking

✅ Market Turnover Monitoring

✅ Top Gainer Identification

✅ Top Loser Identification

---

### Stock Market Monitoring

✅ Stock Symbol Tracking

✅ Company Information

✅ Last Traded Price (LTP)

✅ Change Amount

✅ Change Percentage

✅ Volume Tracking

✅ Last Updated Time

---

### Multi-Threaded Data Collection

✅ Gold/Silver Collector Thread

✅ NEPSE Summary Collector Thread

✅ Stock Price Collector Thread

✅ Independent Thread Monitoring

✅ Thread Status Dashboard

✅ Activity Logs

---

### Scheduler

✅ 1 Minute Collection

✅ 5 Minute Collection

✅ 15 Minute Collection

✅ Continuous Background Collection

---

### Operational Features

✅ Start Collection

✅ Stop Collection

✅ Manual Refresh

✅ Activity Monitoring

✅ Status Dashboard

✅ Export to Excel

---

## 🏗️ Architecture

```text
                       Scheduler
                            │
                            ▼

                   Task.WhenAll()
                            │

        ┌──────────────────────────────────┐
        │                                  │
        ▼                                  ▼

Gold/Silver Thread             NEPSE Thread

        ▼                                  ▼

  Gold Scraper                  Summary Scraper

        │                                  │

        └──────────────┬───────────────────┘
                       │

                       ▼

                Stock Thread
                       │
                       ▼

               Stock Scraper
                       │
                       ▼

               Data Storage
                       │
                       ▼

            Financial Dashboard
```

---

## 📷 Application Screenshots

### Main Application

<img width="1366" height="728" alt="image" src="https://github.com/user-attachments/assets/004de854-dc3d-45a2-a637-f8eedec69571" />

---

## 🖥️ Dashboard Components

### KPI Cards

- Gold Today
- Silver Today
- NEPSE Index
- Market Turnover
- Top Gainer
- Top Loser

### Data Grids

#### Stock Prices

| Field |
|---------|
| Symbol |
| Company |
| LTP |
| Change |
| Change Percentage |
| Volume |
| Updated |

#### Gold / Silver

| Field |
|---------|
| Date |
| Gold Price |
| Silver Price |
| Collection Time |

#### NEPSE Summary

| Field |
|---------|
| Date |
| NEPSE Index |
| Change |
| Volume |
| Turnover |

---

## ⚙️ Technologies Used

### Frontend

- C#
- WinForms
- .NET 10

### Data Collection

- HttpClient
- HtmlAgilityPack

### Multithreading

- Task Parallel Library (TPL)
- Task.WhenAll()
- Async / Await
- CancellationToken

### Database

- SQL Server
- Entity Framework Core

### Export

- ClosedXML
- Excel

---

## 🧵 Multithreading Design

The application uses separate collection threads for different market data sources.

```text
Thread 1
Gold/Silver Collector

Thread 2
NEPSE Summary Collector

Thread 3
Stock Prices Collector
```

Benefits:

- Faster Collection
- Independent Thread Monitoring
- Better Responsiveness
- Improved Scalability
- Fault Isolation

---

## 📊 Sample Dashboard

```text
Gold Today       : Rs 198,500
Silver Today     : Rs 2,250
NEPSE Index      : 2,850.00
Market Turnover  : Rs 8.5 Billion

Top Gainer       : NICA
Top Loser        : SBI
```

---

## 📈 Future Enhancements

### Alerts

```text
Gold > Rs 200,000

Send Notification
```

```text
NEPSE Change > 3%

Generate Alert
```

---

### Dashboard Improvements

- Historical Charts
- Trend Analysis
- Moving Averages
- Sector Analysis
- Portfolio Tracking

---

### Data Collection

- Additional Financial Sources
- Live Market Feed Integration
- API-Based Collection
- Real-Time Streaming

---

## 🎯 Learning Outcomes

This project helped me gain practical experience in:

- Desktop Application Development
- Financial Market Data Processing
- WinForms UI Design
- Multi-Threaded Programming
- Task Parallel Library (TPL)
- Async Programming
- Concurrent Data Collection
- Dashboard Design
- Logging and Monitoring
- SQL Server Integration

---

## 👨‍💻 Author

### Bishal Shrestha


---

⭐ If you found this project useful, please consider giving it a star.

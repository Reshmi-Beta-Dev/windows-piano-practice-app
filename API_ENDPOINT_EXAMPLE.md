# API Endpoint Implementation Example

This document provides an example of how to implement the API endpoint that the Piano Practice Journal application expects.

## Endpoint Specification

**URL**: `POST /practice-sessions`  
**Content-Type**: `application/json`

## Request Format

```json
{
  "sessionId": "123e4567-e89b-12d3-a456-426614174000",
  "startTime": "2024-01-01T10:00:00Z",
  "endTime": "2024-01-01T10:30:00Z",
  "duration": 1800.0,
  "submittedAt": "2024-01-01T10:30:00Z"
}
```

## Response Format

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Session recorded successfully",
  "sessionId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### Error Response (400 Bad Request)

```json
{
  "success": false,
  "message": "Invalid session data: missing endTime",
  "sessionId": null
}
```

## Example Implementations

### ASP.NET Core Web API (C#)

```csharp
[ApiController]
[Route("api/[controller]")]
public class PracticeSessionsController : ControllerBase
{
    private readonly ILogger<PracticeSessionsController> _logger;
    private readonly IPracticeSessionService _sessionService;

    public PracticeSessionsController(
        ILogger<PracticeSessionsController> logger,
        IPracticeSessionService sessionService)
    {
        _logger = logger;
        _sessionService = sessionService;
    }

    [HttpPost]
    public async Task<ActionResult<SessionSubmissionResponse>> SubmitSession(
        [FromBody] SessionSubmissionRequest request)
    {
        try
        {
            _logger.LogInformation("Received practice session: {SessionId}", request.SessionId);

            // Validate request
            if (request.StartTime >= request.EndTime)
            {
                return BadRequest(new SessionSubmissionResponse
                {
                    Success = false,
                    Message = "Start time must be before end time",
                    SessionId = request.SessionId
                });
            }

            // Save to database
            var session = new PracticeSession
            {
                Id = request.SessionId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Duration = TimeSpan.FromSeconds(request.Duration),
                SubmittedAt = request.SubmittedAt
            };

            await _sessionService.SaveSessionAsync(session);

            return Ok(new SessionSubmissionResponse
            {
                Success = true,
                Message = "Session recorded successfully",
                SessionId = request.SessionId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing session {SessionId}", request.SessionId);
            
            return StatusCode(500, new SessionSubmissionResponse
            {
                Success = false,
                Message = "Internal server error",
                SessionId = request.SessionId
            });
        }
    }
}

public class SessionSubmissionRequest
{
    public Guid SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Duration { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class SessionSubmissionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? SessionId { get; set; }
}
```

### Node.js Express (JavaScript)

```javascript
const express = require('express');
const app = express();

app.use(express.json());

app.post('/practice-sessions', async (req, res) => {
    try {
        const { sessionId, startTime, endTime, duration, submittedAt } = req.body;
        
        console.log(`Received practice session: ${sessionId}`);
        
        // Validate request
        if (new Date(startTime) >= new Date(endTime)) {
            return res.status(400).json({
                success: false,
                message: 'Start time must be before end time',
                sessionId: sessionId
            });
        }
        
        // Save to database (example with MongoDB)
        const session = {
            _id: sessionId,
            startTime: new Date(startTime),
            endTime: new Date(endTime),
            duration: duration,
            submittedAt: new Date(submittedAt),
            createdAt: new Date()
        };
        
        await db.collection('practiceSessions').insertOne(session);
        
        res.json({
            success: true,
            message: 'Session recorded successfully',
            sessionId: sessionId
        });
        
    } catch (error) {
        console.error('Error processing session:', error);
        
        res.status(500).json({
            success: false,
            message: 'Internal server error',
            sessionId: req.body.sessionId
        });
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});
```

### Python Flask

```python
from flask import Flask, request, jsonify
from datetime import datetime
import uuid

app = Flask(__name__)

@app.route('/practice-sessions', methods=['POST'])
def submit_session():
    try:
        data = request.get_json()
        
        session_id = data.get('sessionId')
        start_time = datetime.fromisoformat(data.get('startTime').replace('Z', '+00:00'))
        end_time = datetime.fromisoformat(data.get('endTime').replace('Z', '+00:00'))
        duration = data.get('duration')
        submitted_at = datetime.fromisoformat(data.get('submittedAt').replace('Z', '+00:00'))
        
        print(f"Received practice session: {session_id}")
        
        # Validate request
        if start_time >= end_time:
            return jsonify({
                'success': False,
                'message': 'Start time must be before end time',
                'sessionId': session_id
            }), 400
        
        # Save to database (example with SQLite)
        # db.execute('''
        #     INSERT INTO practice_sessions 
        #     (id, start_time, end_time, duration, submitted_at, created_at)
        #     VALUES (?, ?, ?, ?, ?, ?)
        # ''', (session_id, start_time, end_time, duration, submitted_at, datetime.utcnow()))
        
        return jsonify({
            'success': True,
            'message': 'Session recorded successfully',
            'sessionId': session_id
        })
        
    except Exception as e:
        print(f"Error processing session: {e}")
        
        return jsonify({
            'success': False,
            'message': 'Internal server error',
            'sessionId': request.get_json().get('sessionId') if request.get_json() else None
        }), 500

if __name__ == '__main__':
    app.run(debug=True, port=5000)
```

## Database Schema Example

### SQL Server / PostgreSQL

```sql
CREATE TABLE PracticeSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Duration DECIMAL(18,2) NOT NULL, -- Duration in seconds
    SubmittedAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_PracticeSessions_StartTime ON PracticeSessions(StartTime);
CREATE INDEX IX_PracticeSessions_SubmittedAt ON PracticeSessions(SubmittedAt);
```

### MongoDB

```javascript
{
  _id: ObjectId,
  sessionId: String, // GUID from the app
  startTime: Date,
  endTime: Date,
  duration: Number, // Duration in seconds
  submittedAt: Date,
  createdAt: Date
}
```

## Testing the Endpoint

You can test the endpoint using curl:

```bash
curl -X POST http://localhost:3000/practice-sessions \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "123e4567-e89b-12d3-a456-426614174000",
    "startTime": "2024-01-01T10:00:00Z",
    "endTime": "2024-01-01T10:30:00Z",
    "duration": 1800.0,
    "submittedAt": "2024-01-01T10:30:00Z"
  }'
```

## Error Handling

The client application handles these HTTP status codes:

- **200 OK**: Success
- **400 Bad Request**: Invalid data
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Access denied
- **404 Not Found**: Endpoint not found
- **500 Internal Server Error**: Server error
- **Timeout**: Network timeout (configurable, default 30 seconds)

## Security Considerations

1. **Authentication**: Consider adding API key or JWT token authentication
2. **Rate Limiting**: Implement rate limiting to prevent abuse
3. **Input Validation**: Validate all input data
4. **HTTPS**: Use HTTPS in production
5. **CORS**: Configure CORS if needed for web clients

## Monitoring and Logging

Consider implementing:

1. **Request Logging**: Log all incoming requests
2. **Performance Monitoring**: Track response times
3. **Error Tracking**: Monitor and alert on errors
4. **Usage Analytics**: Track session submission patterns

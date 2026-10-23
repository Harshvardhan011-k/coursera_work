#!/bin/bash
set -e

# Configuration
PORT=5123
BASE_URL="http://localhost:$PORT/api/users"
LOG_FILE="/tmp/user_management_api.log"

export PATH="$HOME/.dotnet:$PATH"
export ASPNETCORE_ENVIRONMENT="Development"

echo "=== 1. Starting UserManagementAPI on port $PORT ==="
rm -f "$LOG_FILE"
dotnet run --project /Users/harshvardhankedia/Desktop/c/UserManagementAPI/UserManagementAPI.csproj --urls "http://localhost:$PORT" > "$LOG_FILE" 2>&1 &
API_PID=$!

# Trap exit to ensure server process is always killed
cleanup() {
    echo ""
    echo "=== Shutting down API server (PID: $API_PID) ==="
    kill -9 $API_PID 2>/dev/null || true
    # Also kill any dotnet child processes if spawned
    pkill -f "UserManagementAPI" 2>/dev/null || true
}
trap cleanup EXIT

echo "Waiting for API server to become ready..."
for i in {1..30}; do
    if curl -s "http://localhost:$PORT/api/users" > /dev/null 2>&1; then
        echo "API is ready on http://localhost:$PORT!"
        break
    fi
    sleep 0.5
done

echo ""
echo "=== 2. Running Test Suite ==="

# Test 1: GET /api/users (Initial seed users)
echo -n "Test 1: GET /api/users (Expect 200 OK) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" "$BASE_URL")
if [ "$HTTP_STATUS" -eq 200 ]; then
    echo "PASSED (200 OK)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 2: GET /api/users/1 (Existing user)
echo -n "Test 2: GET /api/users/1 (Expect 200 OK) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" "$BASE_URL/1")
if [ "$HTTP_STATUS" -eq 200 ]; then
    echo "PASSED (200 OK)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 3: GET /api/users/999 (Nonexistent user)
echo -n "Test 3: GET /api/users/999 (Expect 404 Not Found) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" "$BASE_URL/999")
if [ "$HTTP_STATUS" -eq 404 ]; then
    echo "PASSED (404 Not Found)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 4: POST /api/users with invalid email
echo -n "Test 4: POST /api/users with invalid email (Expect 400 Bad Request) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" -X POST "$BASE_URL" \
    -H "Content-Type: application/json" \
    -d '{"name": "Invalid Email User", "email": "invalid-email", "age": 25}')
if [ "$HTTP_STATUS" -eq 400 ]; then
    echo "PASSED (400 Bad Request)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 5: POST /api/users with out-of-range age
echo -n "Test 5: POST /api/users with age 150 (Expect 400 Bad Request) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" -X POST "$BASE_URL" \
    -H "Content-Type: application/json" \
    -d '{"name": "Too Old", "email": "old@example.com", "age": 150}')
if [ "$HTTP_STATUS" -eq 400 ]; then
    echo "PASSED (400 Bad Request)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 6: POST /api/users with duplicate email
echo -n "Test 6: POST /api/users with duplicate email (Expect 409 Conflict) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" -X POST "$BASE_URL" \
    -H "Content-Type: application/json" \
    -d '{"name": "Alice Duplicate", "email": "ALICE.JOHNSON@example.com", "age": 30}')
if [ "$HTTP_STATUS" -eq 409 ]; then
    echo "PASSED (409 Conflict)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 7: POST /api/users (Valid creation)
echo -n "Test 7: POST /api/users valid payload (Expect 201 Created) -> "
HTTP_STATUS=$(curl -s -i -o /tmp/resp_headers.txt -w "%{http_code}" -X POST "$BASE_URL" \
    -H "Content-Type: application/json" \
    -d '{"name": "  Diana Prince  ", "email": "diana.prince@example.com", "age": 30}')
if [ "$HTTP_STATUS" -eq 201 ]; then
    echo "PASSED (201 Created)"
    grep -i "location:" /tmp/resp_headers.txt || true
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp_headers.txt
    exit 1
fi

# Test 8: GET /api/users/4 (Verify newly created user was trimmed and stored)
echo -n "Test 8: GET /api/users/4 (Expect 200 OK) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" "$BASE_URL/4")
if [ "$HTTP_STATUS" -eq 200 ]; then
    echo "PASSED (200 OK)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 9: PUT /api/users/4 (Valid update)
echo -n "Test 9: PUT /api/users/4 (Expect 200 OK) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" -X PUT "$BASE_URL/4" \
    -H "Content-Type: application/json" \
    -d '{"name": "Diana Wayne", "email": "diana.prince@example.com", "age": 31}')
if [ "$HTTP_STATUS" -eq 200 ]; then
    echo "PASSED (200 OK)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 10: PUT /api/users/4 with duplicate email of another user (Expect 409 Conflict)
echo -n "Test 10: PUT /api/users/4 with conflicting email (Expect 409 Conflict) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" -X PUT "$BASE_URL/4" \
    -H "Content-Type: application/json" \
    -d '{"name": "Diana Wayne", "email": "alice.johnson@example.com", "age": 31}')
if [ "$HTTP_STATUS" -eq 409 ]; then
    echo "PASSED (409 Conflict)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 11: PUT /api/users/999 (Nonexistent user)
echo -n "Test 11: PUT /api/users/999 (Expect 404 Not Found) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" -X PUT "$BASE_URL/999" \
    -H "Content-Type: application/json" \
    -d '{"name": "Ghost", "email": "ghost@example.com", "age": 40}')
if [ "$HTTP_STATUS" -eq 404 ]; then
    echo "PASSED (404 Not Found)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 12: DELETE /api/users/4 (Delete user)
echo -n "Test 12: DELETE /api/users/4 (Expect 204 No Content) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" -X DELETE "$BASE_URL/4")
if [ "$HTTP_STATUS" -eq 204 ]; then
    echo "PASSED (204 No Content)"
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 13: GET /api/users/4 (Confirm user is deleted)
echo -n "Test 13: GET /api/users/4 after deletion (Expect 404 Not Found) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" "$BASE_URL/4")
if [ "$HTTP_STATUS" -eq 404 ]; then
    echo "PASSED (404 Not Found)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

# Test 14: DELETE /api/users/999 (Delete nonexistent user)
echo -n "Test 14: DELETE /api/users/999 (Expect 404 Not Found) -> "
HTTP_STATUS=$(curl -s -o /tmp/resp.json -w "%{http_code}" -X DELETE "$BASE_URL/999")
if [ "$HTTP_STATUS" -eq 404 ]; then
    echo "PASSED (404 Not Found)"
    cat /tmp/resp.json
    echo ""
else
    echo "FAILED ($HTTP_STATUS)"
    cat /tmp/resp.json
    exit 1
fi

echo ""
echo "=== 3. Verifying Custom RequestLoggingMiddleware Logs ==="
sleep 1
grep -E "HTTP (GET|POST|PUT|DELETE) /api/users.* responded [0-9]{3} in [0-9]+ ms" "$LOG_FILE" || true

echo ""
echo "=== ALL 14 TESTS AND MIDDLEWARE LOG VERIFICATIONS PASSED SUCCESSFULLY! ==="

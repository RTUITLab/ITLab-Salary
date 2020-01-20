#!/bin/bash

HTTP_STATUS="$(curl -IL --silent http://test-salary-api | grep HTTP | grep -P '\d\d\d' -o )"; 
echo "$HTTP_STATUS"
while [ "$HTTP_STATUS" != "404" ]; do
    echo "not up $HTTP_STATUS"
    sleep 1
    HTTP_STATUS="$(curl -IL --silent http://test-salary-api | grep HTTP | grep -P '\d\d\d' -o )"; 
done
echo "DONE $HTTP_STATUS"
#!/bin/bash

# curl heartbeat
curl https://central-il-chi-101.cseretail.com/heartbeat/17; echo "   central-il-chi-101" &
curl https://central-il-chi-102.cseretail.com/heartbeat/17; echo "   central-il-chi-102" &
curl https://east-ny-nyc-101.cseretail.com/heartbeat/17; echo "   east-ny-nyc-101" &
curl https://east-ny-nyc-102.cseretail.com/heartbeat/17; echo "   east-ny-nyc-102" &
curl https://west-or-pdx-101.cseretail.com/heartbeat/17; echo "   west-or-pdx-101" &
curl https://west-or-pdx-102.cseretail.com/heartbeat/17; echo "   west-or-pdx-102" &

# curl the app
curl https://central-il-chi-101.cseretail.com/api/benchmark/18; echo "  central-il-chi-101" &
curl https://central-il-chi-102.cseretail.com/api/benchmark/18; echo "  central-il-chi-102" &

#curl https://east-ny-nyc-101.cseretail.com/api/benchmark/18; echo "  east-ny-nyc-101" &
#curl https://east-ny-nyc-102.cseretail.com/api/benchmark/18; echo "  east-ny-nyc-102" &

#curl https://west-or-pdx-101.cseretail.com/api/benchmark/18; echo "  west-or-pdx-101" &
#curl https://west-or-pdx-102.cseretail.com/api/benchmark/18; echo "  west-or-pdx-102" &

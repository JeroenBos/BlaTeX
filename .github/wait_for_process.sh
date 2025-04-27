#!/bin/bash

# This circumvents inability of built-in `wait` waiting for processes started by other shells.
# Exits with the exit code of the specified PID, optionally printing its logs.
# Usage:
#
#     ./.github/wait_for_process.sh <PID> [<logfile>]
#
# Expects the exit code of the specified PID to be logged in the form `pid_{pid}={exit_code}` to `/tmp/exit_codes.env`.
# 

if [[ -z "$1" ]]; then
  exit 12 # no processId specified; fail.
fi

if [[ $# -gt 2 ]]; then 
  >&2 echo too many arguments specified; expected at most 2.
  exit 12
fi

pid="$1"
logFile="${2:-}"

# check if a number was provided
if ! [[ $pid =~ ^[0-9]+$ ]] ; then
  >&2 echo "First argument was expected to be a number; got '$pid'"
  exit 12
fi

# wait for process to exit
while [[ -d "/proc/$pid" ]]; 
  do sleep 1
done

# retrieve exit code of process
source /tmp/exit_codes.env &> /dev/null
variable="pid_${pid}"
exit_code="${!variable}"

if [[ -z "$exit_code" ]]; then
  >&2 echo "No exit code found in special .env file for pid '$pid'. What we have is:"
  >&2 cat /tmp/exit_codes.env || true
  >&2 echo
  exit 12
fi

if [[ ! -z "${logFile}" ]]; then
    cat "${logFile}"
fi

exit "$exit_code"

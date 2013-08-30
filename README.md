jira-toggl-sync (ALPHA)
=======================

A simple tool to synchronize Toggl time entries into Atlassian JIRA's work log entries

Open a command line in the tool directory and type:

> jira-toggl-sync

The tool will prompt for a Toggl API key (which can be found in your profile page at the bottom), and for a JIRA instance URL, username, password and the JIRA project keys you want.

A list of issues will then be shown. You can accept to enter a worklog for it by pressing "y".

Only issues from the last 14 days with a matching JIRA key will be selected.

Copyright and license
=====================

Copyright 2013 Christian Rondeau, under the GPL license.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>

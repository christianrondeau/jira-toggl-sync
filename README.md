jira-toggl-sync
===============

[![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/fg2po52e24qj2bc9?svg=true)](https://ci.appveyor.com/project/christianrondeau/jira-toggl-sync)

A simple tool to synchronize Toggl time entries into Atlassian JIRA's work log entries

Open a command line in the tool directory and type:

> jira-toggl-sync

The tool will prompt for a Toggl API key (which can be found in your profile page at the bottom), and for a JIRA instance URL, username, password and the JIRA project keys you want.

A list of issues will then be shown. You can accept to enter a worklog for it by pressing "y".

Only issues from the last 14 days with a matching JIRA key will be selected.

Parameters
============
### jira-decription-template
When Toggl's time entry is converted to JIRA's work log entry, additional information from toggl time entry can be stored in JIRA's work log comment.

Following placeholders that can be used in template:
- **{{toggl:id}}** - Required to be present somewhere in the template
- **{{toggl:description}}**
- **{{toggl:createdWith}}**
- **{{toggl:isBillable}}**
- **{{toggl:projectId}}**
- **{{toggl:tagNames}}**
- **{{toggl:taskId}}**
- **{{toggl:updatedOn}}**

Default placeholder: `{{toggl:id}}\r\n{{toggl:description}}`

Contributors
============

* [Christian Rondeau](https://github.com/christianrondeau)
* [Dominic St-Jacques](https://github.com/dstj)

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

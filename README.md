# Todo

- Write the readme
- use default server, if setting empty.
- Remove: delete values using the old fashion method. if cant make it work with the classic method.
- Cleanup logs and make them more comprehensive
- When running in normal operation:
	- make a single list of tags, or use a much higher tag count for the list(s)
	- use a single thread and no multi threads


#Settings sample:
  <PIReplay.Settings.General>
        <setting name="ReplayTimeOffsetDays" serializeAs="String">
            <value>365</value>
        </setting>
        <setting name="BackFillHoursPerDataChunk" serializeAs="String">
            <value>24</value>
        </setting>
        <setting name="TagsChunkSize" serializeAs="String">
            <value>500</value>
        </setting>
        <setting name="DataCollectionFrequencySeconds" serializeAs="String">
            <value>5</value>
        </setting>
        <setting name="ServerName" serializeAs="String">
            <value>pidemo2016</value>
        </setting>
        <setting name="TagQueryString" serializeAs="String">
            <value>tag:=simulator.random.*</value>
        </setting>
        <setting name="BulkPageSize" serializeAs="String">
            <value>1000</value>
        </setting>
        <setting name="BulkParallelChunkSize" serializeAs="String">
            <value>100</value>
        </setting>
        <setting name="BackfillDefaultStartTime" serializeAs="String">
            <value>08/12/2016 17:00:00</value>
        </setting>
        <setting name="SleepTimeBetweenChunksMs" serializeAs="String">
            <value>200</value>
        </setting>

#License
 
    Copyright 2015 Patrice Thivierge Fortin
 
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
 
    http://www.apache.org/licenses/LICENSE-2.0
 
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.

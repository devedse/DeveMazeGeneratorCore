namespace DeveMazeGeneratorCore.Coaster3MF
{
    public static class BambuStudioMetadata
    {
        public static string CutInformation => """
            <?xml version="1.0" encoding="utf-8"?>
            <objects>
             <object id="1">
              <cut_id id="0" check_sum="1" connectors_cnt="0"/>
             </object>
            </objects>
            """;

        public static string SliceInfo => """
            <?xml version="1.0" encoding="UTF-8"?>
            <config>
              <header>
                <header_item key="X-BBL-Client-Type" value="slicer"/>
                <header_item key="X-BBL-Client-Version" value="02.01.01.52"/>
              </header>
            </config>
            """;

        public static string ProjectSettings => """
            {
                "from": "project",
                "name": "project_settings",
                "version": "02.01.01.52"
            }
            """;
    }
}
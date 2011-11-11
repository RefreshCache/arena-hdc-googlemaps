<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="html" indent="yes"/>

    <xsl:template match="/groups">
      <table id="sgResults" class="sgl_results tablesorter" cellspacing="0" cellpadding="5" rules="rows" border="1" style="border-collapse: collapse;">
        <thead>
          <tr class="sglHeader" style="font-weight: bold;">
            <th>Group</th>
            <th>Meeting Day</th>
            <th>Time</th>
            <th>Type</th>
            <th>Topic</th>
            <th>Avg Age</th>
            <!--<th>Notes</th>-->
            <xsl:if test="/groups/@has_distance = 1">
              <th>Distance</th>
            </xsl:if>
            <th>View Map</th>
          </tr>
        </thead>
        <tbody>
          <xsl:for-each select="group">
            <tr class="sglRow">
              <xsl:attribute name="onclick">window.location = 'default.aspx?page=<xsl:value-of select="/groups/@registration_page"/>&amp;group=<xsl:value-of select="@id"/>';</xsl:attribute>
              <td>
                <xsl:value-of select="@name"/>
              </td>
              <td>
                <xsl:value-of select="@meetingday"/>
              </td>
              <td>
                <xsl:value-of select="@meetingstarttime"/>
              </td>
              <td>
                <xsl:value-of select="@type"/>
              </td>
              <td>
                <xsl:value-of select="@topic"/>
              </td>
              <td>
                <xsl:value-of select="@averageage"/>
              </td>
              <!--
              <td>
                <xsl:value-of select="@notes"/>
              </td>
              -->
              <xsl:if test="/groups/@has_distance = 1">
                <td>
                  <xsl:value-of select="@distance"/>
                </td>
              </xsl:if>
              <td class="sglMapLink">
                <a>
                  <xsl:attribute name="href"><xsl:value-of select="@id"/></xsl:attribute>
                  <xsl:text disable-output-escaping="yes">View Map</xsl:text>
                </a>
              </td>
            </tr>
          </xsl:for-each>
        </tbody>
      </table>

      <div id="sgResults_pager" class="pager">
        <img src="UserControls/Custom/HDC/GoogleMaps/Images/first.png" class="first"/>
        <img src="UserControls/Custom/HDC/GoogleMaps/Images/prev.png" class="prev"/>
        <input type="text" class="pagedisplay" readonly="" />
        <img src="UserControls/Custom/HDC/GoogleMaps/Images/next.png" class="next"/>
        <img src="UserControls/Custom/HDC/GoogleMaps/Images/last.png" class="last"/>
        <select class="pagesize" style="display: none">
          <option selected="selected"  value="10">10</option>
          <option value="25">25</option>
          <option value="50">50</option>
        </select>
      </div>

      <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  $(document).ready(function() {
    $('#sgResults')
      .tablesorter({widthFixed: true, widgets: ['zebra']})
      .tablesorterPager({container: $('#sgResults_pager'), positionFixed: false, size: 10});
  });
  $(document).ready(function () {
    $('.sglMapLink').click(function (e) {
      e.preventDefault();
      e.stopPropagation();
      _ShowGroupPopup(null, $(this).find('a').attr('href'));
    });
  });
</script>
]]></xsl:text>
    </xsl:template>

</xsl:stylesheet>

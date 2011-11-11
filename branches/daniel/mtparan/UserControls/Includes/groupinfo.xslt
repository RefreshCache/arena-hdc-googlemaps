<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/Data/group">
    <div style="font-size: 12px;">
      <!-- Small Group Name -->
      <p style="text-align: center;">
        <b><xsl:value-of select="@name"/></b>
      </p>
      
      <!-- Small Group Picture -->
      <xsl:if test="@picture != ''">
        <p align="center">
          <img>
            <xsl:attribute name="src">
              <xsl:value-of select="@picture"/>
              <xsl:text>&amp;width=100&amp;height=100</xsl:text>
            </xsl:attribute>
          </img>
        </p>
      </xsl:if>

      <table>
        <!-- Meeting Day -->
        <tr>
          <td>
            <xsl:value-of select="/Data/@meetingday_caption"/>
            <xsl:text>: </xsl:text>
          </td>
          <td>
            <xsl:value-of select="@meetingday"/>
            <xsl:if test="@meetingstarttime != '12:00 AM'">
              <xsl:text> </xsl:text>
              <xsl:value-of select="@meetingstarttime"/>
            </xsl:if>
          </td>
        </tr>
        <!-- Group Type -->
        <tr>
          <td>
            <xsl:value-of select="/Data/@type_caption"/>
            <xsl:text>: </xsl:text>
          </td>
          <td>
            <xsl:value-of select="@type"/>
          </td>
        </tr>
        <!-- Group Topic -->
        <tr>
          <td>
            <xsl:value-of select="/Data/@topic_caption"/>
            <xsl:text>: </xsl:text>
          </td>
          <td>
            <xsl:value-of select="@topic"/>
          </td>
        </tr>
        <!-- Average Age -->
        <tr>
          <td>
            <xsl:text>Average Age: </xsl:text>
          </td>
          <td>
            <xsl:value-of select="@averageage"/>
          </td>
        </tr>
        <xsl:if test="@notes != ''">
          <tr>
            <td colspan="2">
              <xsl:text>Notes: </xsl:text>
              <xsl:value-of select="@notes"/>
            </td>
          </tr>
        </xsl:if>
      </table>
    </div>
  </xsl:template>
</xsl:stylesheet>

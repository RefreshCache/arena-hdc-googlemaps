<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:param name="detailsAsPopup">no</xsl:param>
  <xsl:param name="registrationAsPopup">yes</xsl:param>

  <xsl:template match="/groups">
    <!-- This should probably be in a separate css, but for the moment if lives here. -->
    <xsl:text disable-output-escaping="yes"><![CDATA[
<style type="text/css">
div.sgl_row {
  font-size: 12px;
  color: white;
}
div.sgl_row span {
  padding-top: 10px;
  padding-bottom: 10px;
  background-color: #5050d0;
  display: inline-block;
  overflow-x: hidden;
  white-space: nowrap;
}
span.sglr_showdetails,span.sglr_map,span.sglr_info {
  border-left: 1px solid gray;
  cursor: pointer;
  text-align: center;
}
span.sglr_name { width: 225px; padding-left: 10px; }
span.sglr_showdetails { width: 100px; }
span.sglr_map { width: 100px; }
span.sglr_info { width: 80px; }
div.sglr_details {
  margin-left: 5px;
  margin-right: 5px;
  margin-bottom: 5px;
  width: 508px;
  color: white;
  height: 0px;
  overflow: hidden;
}

/* This is only used when displaying inline */
div.sglr_details div.sglr_detailcontent {
  background-color: #5050d0;
  border-top: 1px solid gray;
}

/* This is always used */
div.sglr_detailcontent {
  padding: 7px;
  font-size: 12px;
}

/* Make the group information display look pretty. Space out the title and data. */
span.sglrc_title { float: left; text-align: right; width: 80px; }
span.sglrc_data { display: block; padding-left: 85px; }
</style>
]]></xsl:text>
 
    <!-- Hide the address panel if there is no map. -->
    <xsl:if test="/groups/@has_map != 'True'">
      <xsl:text disable-output-escaping="yes"><![CDATA[
<style type="text/css">
div.sgl_address { display: none; }
</style>
]]></xsl:text>
    </xsl:if>

    <div class="sgl_results" cellspacing="0" cellpadding="5" rules="rows" border="0">
      <xsl:for-each select="group">
        <xsl:sort select="@name"/>

        <!-- The first div contains the name, 'Show Details', 'Show On Map' and 'More Info' buttons. -->
        <div class="sgl_row">
          <span class="sglr_name">
            <xsl:value-of select="@name"/>
          </span>
          <span class="sglr_showdetails">Show Details</span>
          <span class="sglr_map">
            <xsl:attribute name="data-id">
              <xsl:value-of select="@id"/>
            </xsl:attribute>
            <xsl:attribute name="data-latitude">
              <xsl:value-of select="@latitude"/>
            </xsl:attribute>
            <xsl:attribute name="data-longitude">
              <xsl:value-of select="@longitude"/>
            </xsl:attribute>
            <xsl:text>Show On Map</xsl:text>
          </span>
          <span class="sglr_info">
            <xsl:attribute name="data-id">
              <xsl:value-of select="@id"/>
            </xsl:attribute>
            <xsl:text>More Info</xsl:text>
          </span>
        </div>

        <!-- The second div contains the detail content information that is displayed
        when the 'Show Details' button is clicked.. -->
        <div class="sglr_details">
          <div class="sglr_detailcontent">
            <xsl:attribute name="id">
              <xsl:text>sglr_detailcontent_</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:attribute>
            <div>
              <span class="sglrc_title">
                <xsl:text>Leader:</xsl:text>
              </span>
              <span class="sglrc_data">
                <xsl:value-of select="@leadername"/>
                <xsl:text disable-output-escaping="yes">&amp;nbsp;</xsl:text>
              </span>
            </div>
            <div>
              <span class="sglrc_title">
                <xsl:text>Meeting Day:</xsl:text>
              </span>
              <span class="sglrc_data">
                <xsl:value-of select="@meetingday"/>
                <xsl:if test="@meetingstarttime != '12:00 AM'">
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="@meetingstarttime"/>
                </xsl:if>
              </span>
            </div>
            <div>
              <span class="sglrc_title">
                <xsl:text>Age Range:</xsl:text>
              </span>
              <span class="sglrc_data">
                <xsl:value-of select="@agerange"/>
              </span>
            </div>
            <div>
              <span class="sglrc_title">
                <xsl:text>Type:</xsl:text>
              </span>
              <span class="sglrc_data">
                <xsl:value-of select="@type"/>
              </span>
            </div>
            <xsl:if test="@description != ''">
              <div>
                <span class="sglrc_title">
                  <xsl:text>Description:</xsl:text>
                </span>
                <span class="sglrc_data">
                  <xsl:value-of select="@description"/>
                </span>
              </div>
            </xsl:if>
            <xsl:if test="@schedule != ''">
              <div>
                <span class="sglrc_title">
                  <xsl:text>Schedule:</xsl:text>
                </span>
                <span class="sglrc_data">
                  <xsl:value-of select="@schedule"/>
                </span>
              </div>
            </xsl:if>
            <xsl:if test="@notes != ''">
              <div>
                <span class="sglrc_title">
                  <xsl:text>Notes:</xsl:text>
                </span>
                <span class="sglrc_data">
                  <xsl:value-of select="@notes"/>
                </span>
              </div>
            </xsl:if>
          </div>
        </div>
      </xsl:for-each>
    </div>

    <!-- The javascript to prepare the colorbox information. -->
    <xsl:if test="/groups/@has_map != 'True'">
      <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  var sgl_colorboxinit = false;

  $(document).ready(function () {
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = 'UserControls/Custom/HDC/GoogleMaps/Includes/jquery.colorbox-min.js';
    $('head').append(script);
    $('head').append('<link rel="stylesheet" href="UserControls/Custom/HDC/GoogleMaps/Includes/colorbox.css" type="text/css" />');
  });
</script>
]]></xsl:text>
    </xsl:if>

    <!-- Show registration page inline -->
    <xsl:if test="$registrationAsPopup != 'yes'">
      <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  $(document).ready(function() {
    $('.sglr_info').click(function (e) {
      window.location = 'default.aspx?page=]]></xsl:text>
<xsl:value-of select="/groups/@registration_page" />
<xsl:text disable-output-escaping="yes"><![CDATA[&group=' + $(this).attr('data-id');
    });
  });
</script>
]]></xsl:text>
    </xsl:if>

    <!-- Show registration page as popup -->
    <xsl:if test="$registrationAsPopup = 'yes'">
      <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  $(document).ready(function() {
    $('.sglr_info').click(function (e) {
      if (sgl_colorboxinit == false) {
        $.colorbox.init();
        sgl_colorboxinit = true;
      }

      var url = 'default.aspx?page=]]></xsl:text>
      <xsl:value-of select="/groups/@registration_page" />
      <xsl:text disable-output-escaping="yes"><![CDATA[&group=' + $(this).attr('data-id');

      $.colorbox({iframe: true, href: url, opacity: 0.7, width: '450px', height: '600px', initialWidth: '350px', initialHeight: '500px'});
    });
  });
</script>
]]></xsl:text>
    </xsl:if>

    <!-- The javascript that is used to do final initialization. -->
    <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  var original_RegisterSmallGroup = RegisterSmallGroup;
  function RegisterSmallGroup(gm, marker) {
    marker.icon = "Custom/MtParan/star.png";
    original_RegisterSmallGroup(gm, marker);
  }
</script>
]]></xsl:text>
    
    <!-- The javascript to show the group on a map when the central map is available. -->
    <xsl:if test="/groups/@has_map = 'True'">
      <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  $(document).ready(function() {
    $('.sglr_map').click(function (e) {
      _ShowGroupPopup(null, $(this).attr('data-id'));
    });
  });
</script>
]]></xsl:text>
    </xsl:if>

    <!-- The javascript to show the group on a map when the central map is not available. Use colorbox. -->
    <xsl:if test="/groups/@has_map != 'True'">
      <div style="overflow: hidden; width: 0px; height: 0px;">
        <div id="mtparan_map" style="width: 420px; height: 360px;"></div>
      </div>
      <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  var mtparan_map, mtparan_marker = null;
  $(document).ready(function() {
    var myOptions = {
      zoom: 12,
      center: new google.maps.LatLng(0, 0),
      mapTypeId: google.maps.MapTypeId.ROADMAP,
      disableDefaultUI: true,
      draggable: false,
      scrollwheel: false,
      keyboardShortcuts: false,
      maxZoom: 14
    };

    mtparan_map = new google.maps.Map(document.getElementById('mtparan_map'), myOptions);
  });

  $(document).ready(function() {
    $('.sglr_map').click(function (e) {
      if (sgl_colorboxinit == false) {
        $.colorbox.init();
        sgl_colorboxinit = true;
      }

      /* Update the center of the map and the marker location. */
      var center = new google.maps.LatLng($(this).attr('data-latitude'), $(this).attr('data-longitude'));
      mtparan_map.setCenter(center);
      if (mtparan_marker != null)
        mtparan_marker.setMap(null);
      mtparan_marker = new google.maps.Marker({position: center, map: mtparan_map, title: $(this).prev().prev().text(), icon: 'Custom/MtParan/star.png'});

      $.colorbox({inline: true, href: '#mtparan_map', opacity: 0.7, initialWidth: '300px', initialHeight: '100px'});
    });
  });
</script>
]]></xsl:text>
    </xsl:if>

    <!-- The javascript to show the group details as a popup. -->
    <xsl:if test="$detailsAsPopup = 'yes'">
      <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  $(document).ready(function() {
    $('.sglr_showdetails').click(function (e) {
      if (sgl_colorboxinit == false) {
        $.colorbox.init();
        sgl_colorboxinit = true;
      }
      $.colorbox({inline: true, href: '#' + $(this).parent().next().children().attr('id'), maxWidth: '550px', opacity: 0.7, initialWidth: '300px', initialHeight: '100px'});
    });
  });
</script>
]]></xsl:text>
    </xsl:if>

    <!-- The javascript to show the group details inline. -->
    <xsl:if test="$detailsAsPopup != 'yes'">
      <xsl:text disable-output-escaping="yes"><![CDATA[
<script type="text/javascript">
  $(document).ready(function() {
    $('.sglr_showdetails').click(function (e) {
      if ($(this).parent().next().height() == 0) {
        var content = $(this).parent().next().children();
        var ptop = parseInt(content.css("padding-top"));
        var pbottom = parseInt(content.css("padding-bottom"));

        $(this).parent().next().animate({height: content.height() + ptop + pbottom}, 200);
        $(this).text('Hide Details');
      }
      else {
        $(this).parent().next().animate({height: 0}, 200);
        $(this).text('Show Details');
      }
    });
  });
</script>
]]></xsl:text>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
                
>
  <xsl:output method= "text" indent="no" encoding="windows-1251"/>
  
  <xsl:strip-space elements="*"/>

  <xsl:variable name="newline">
    <xsl:text>
    </xsl:text>
  </xsl:variable>

  <xsl:variable name="tab">
    <xsl:text>&#x9;</xsl:text>  
  </xsl:variable>

  <xsl:key name="TVDate" match="programme" use="date" />

  <xsl:template match="tv">
    <!--<AllChannels> -->
    <!--<xsl:apply-templates select="channel"/>-->
    <xsl:apply-templates select="programme[generate-id(.)=generate-id(key('TVDate',date)[1])]"/>
    
    <!-- </AllChannels> -->
  </xsl:template>
<!--
  <xsl:template match="channel">
    <xsl:value-of select ="display-name"/>  
    <xsl:value-of select="$newline"/>
  </xsl:template>
  -->
  <xsl:template match="programme">
    <!--<xsl:value-of select="msxsl:format-date(date, 'dddd, dd MMMM yyyy')" />-->
    <xsl:value-of select="msxsl:format-date(date, 'dd.MM.yyyy')" />
    <xsl:value-of select="$newline"/>
    <xsl:apply-templates mode="next" select="key('TVDate',date)"/>
    <xsl:value-of select="$newline"/>
  </xsl:template>

    
  <xsl:template match="programme" mode="next">
    <xsl:value-of select="substring(@start, 9, 2)" />:<xsl:value-of select="substring(@start, 11, 2)" />
    <xsl:value-of select="$tab"/>
    <xsl:value-of select="substring(@stop, 9, 2)" />:<xsl:value-of select="substring(@stop, 11, 2)" />
    <xsl:value-of select="$tab"/>
    <xsl:value-of select="title" />
    <xsl:value-of select="$tab"/>
    <xsl:for-each select="category">
      <xsl:value-of select="text()"/>
      <xsl:if test="position() != last()">
        <xsl:text>; </xsl:text>
      </xsl:if>
    </xsl:for-each>
    <xsl:value-of select="$newline"/>
  </xsl:template>

  <xsl:template match="programme[generate-id(.)=generate-id(key('TVDate',date)[last()])]" mode="next">
    <xsl:value-of select="substring(@start, 9, 2)" />:<xsl:value-of select="substring(@start, 11, 2)" />
    <xsl:value-of select="$tab"/>
    <xsl:value-of select="substring(@stop, 9, 2)" />:<xsl:value-of select="substring(@stop, 11, 2)" />
    <xsl:value-of select="$tab"/>
    <xsl:value-of select="title" />
    <xsl:value-of select="$tab"/>
    <xsl:value-of select="category"/>
    <!--
    <xsl:value-of select="$newline"/>До <xsl:value-of select="substring(@stop, 9, 2)" />:<xsl:value-of select="substring(@stop, 11, 2)" />
    <xsl:value-of select="$newline"/>
    -->
  </xsl:template>

</xsl:stylesheet>

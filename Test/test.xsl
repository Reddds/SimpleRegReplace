<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:myObj="urn:price-conv"
                version="2.0">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="no" version="1.0"/> 
  
<xsl:template match="topic">
    <page>
      <xsl:apply-templates select="page"/>
    </page>
</xsl:template>

<xsl:template match="page">
 <!-- <xsl:variable name="filename" select="concat('output1/',position(),'.xml')" />
  <xsl:result-document href="{$filename}" format="xml">-->
    <content title="{@soundname}" num="{position()}">
      <price>
         <xsl:value-of select="myObj:NewPriceFunc(100, 5)"/>
      </price>
      <xsl:message terminate="no">
        <xsl:value-of select="position()"/>
      </xsl:message>
      <xsl:apply-templates select="sound"/>
      <xsl:apply-templates select="description"/>
    </content>

</xsl:template>

<xsl:template match="description">
	<lang id="{@language}">
		<xsl:apply-templates />
	</lang>
</xsl:template>
  
 <xsl:template match="sound">
	<module id="{text()}">
    <xsl:value-of select="."/>
	</module>
</xsl:template>
 
 <xsl:template match="description/phonem">
   <phonem>
		<xsl:apply-templates />
	</phonem>
</xsl:template>

  <xsl:template match="description/b">
    <b>
      <xsl:apply-templates />
    </b>
  </xsl:template>

</xsl:stylesheet>
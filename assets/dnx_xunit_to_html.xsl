<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html"/>
    <xsl:template match="/">
        <xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">]]></xsl:text>
        <html>
            <head>
                <title>
                    xUnit.net Test Results - <xsl:value-of select="@name"/>
                </title>
                <style type="text/css">
                    body { font-family: Calibri, Verdana, Arial, sans-serif; background-color: White; color: Black; }
                    h2,h3,h4,h5 { margin: 0; padding: 0; }
                    h3 { font-weight: normal; }
                    h5 { font-weight: normal; font-style: italic; margin-bottom: 0.75em; }
                    pre { font-family: Consolas; font-size: 85%; margin: 0 0 0 1em; padding: 0; }
                    .divided { border-top: solid 1px #f0f5fa; padding-top: 0.5em; }
                    .row, .altrow { padding: 0.1em 0.3em; }
                    .row { background-color: #f0f5fa; }
                    .altrow { background-color: #e1ebf4; }
                    .success, .failure, .skipped { font-family: Arial Unicode MS; font-weight: normal; float: left; width: 1em; display: block; }
                    .success { color: #0c0; }
                    .failure { color: #c00; }
                    .skipped { color: #cc0; }
                    .timing { float: right; }
                    .indent { margin: 0.25em 0 0.5em 2em; }
                    .indenttest { margin: 0.25em 0 0.5em 1em; }
                    .clickable { cursor: pointer; }
                    .testcount { font-size: 85%; }
                </style>
                <script language="javascript">
                    function ToggleClass(id) {
                        var elem = document.getElementById(id);
                        if (elem.style.display == "none") {
                            elem.style.display = "block";
                        }
                        else {
                            elem.style.display = "none";
                        }
                    }
                    function EnsureBlock(id) {
                        var elem = document.getElementById(id);
                        if (elem.style.display == "none") {
                            elem.style.display = "block";
                        }
                    }
                </script>
            </head>
            <body>
                <h3 class="divided"><b>Assemblies Run</b></h3>
                <xsl:apply-templates select="//assembly"/>

                <h3 class="divided"><b>Summary</b></h3>
                <div>
                    Tests run: <a href="#all"><b><xsl:value-of select="sum(//assembly/@total)"/></b></a> &#160;
                    Failures: <a href="#failures"><b><xsl:value-of select="sum(//assembly/@failed)"/></b></a>,
                    Skipped: <a href="#skipped"><b><xsl:value-of select="sum(//assembly/@skipped)"/></b></a>,
                    Run time: <b><xsl:value-of select="sum(//assembly/@time)"/>s</b>
                </div>
                <xsl:if test="//assembly/collection/test[@result='Fail']">
                    <br />
                    <h2><a name="failures"></a>Failed tests</h2>
                    <xsl:apply-templates select="//assembly/collection/test[@result='Fail']"><xsl:sort select="@name"/></xsl:apply-templates>
                </xsl:if>
                <xsl:if test="//assembly/collection/failure">
                    <br />
                    <h2><a name="failures"></a>Failed fixtures</h2>
                    <xsl:apply-templates select="//assembly/collection/failure"><xsl:sort select="../@name"/></xsl:apply-templates>
                </xsl:if>
                <xsl:if test="//assembly/@skipped > 0">
                    <br />
                    <h2><a name="skipped"></a>Skipped tests</h2>
                    <xsl:apply-templates select="//assembly/collection/test[@result='Skip']"><xsl:sort select="@name"/></xsl:apply-templates>
                </xsl:if>
                <br />
                <h2><a name="all"></a>All tests</h2>
                <h5>Click test collection name to expand/collapse test details</h5>
                <xsl:apply-templates select="//assembly/collection"><xsl:sort select="@name"/></xsl:apply-templates>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="assembly">
        <div><xsl:value-of select="@name"/></div>
    </xsl:template>

    <xsl:template match="test">
        
        <div>
            <xsl:attribute name="class"><xsl:if test="(position() mod 2 = 0)">alt</xsl:if>row</xsl:attribute>
           
            <xsl:if test="@result!='Skip'"><span class="timing"><xsl:value-of select="@time"/>s</span></xsl:if>
            <xsl:if test="@result='Skip'"><span class="timing">Skipped</span><span class="skipped">&#x2762;</span></xsl:if>
            <xsl:if test="@result='Fail'"><span class="failure">&#x2718;</span></xsl:if>
            <xsl:if test="@result='Pass'"><span class="success">&#x2714;</span></xsl:if>
             <span class="clickable">
                <xsl:attribute name="onclick">ToggleClass('test<xsl:value-of select="generate-id()"/>')</xsl:attribute>
            &#160;<xsl:value-of select="@name"/>
            </span>
           

        <div >

            <xsl:if test="@result='Pass'"><xsl:attribute name="style">display: none;</xsl:attribute></xsl:if>
            <xsl:attribute name="id">test<xsl:value-of select="generate-id()"/></xsl:attribute>

             <xsl:if test="child::node()/message"> : <xsl:value-of select="child::node()/message"/></xsl:if>
            <br clear="all" />
            <xsl:if test="failure/stack-trace">
                <pre><xsl:value-of select="failure/stack-trace"/></pre>
            </xsl:if>
            <xsl:if test="output">
                <h4>Output</h4>
                <pre><xsl:value-of select="output"/></pre>
            </xsl:if>
        </div>

        </div>
    </xsl:template>

    <xsl:template match="failure">
        <span class="failure">&#x2718;</span> <xsl:value-of select="../@name"/> : <xsl:value-of select="message"/><br clear="all"/>
        Stack Trace:<br />
        <pre><xsl:value-of select="stack-trace"/></pre>
    </xsl:template>

    <xsl:template match="collection">
        <h3>
            <span class="timing"><xsl:value-of select="@time"/>s</span>
            <span class="clickable">
                <xsl:attribute name="onclick">ToggleClass('collection<xsl:value-of select="generate-id()"/>')</xsl:attribute>
                <xsl:attribute name="ondblclick">ToggleClass('collection<xsl:value-of select="generate-id()"/>')</xsl:attribute>
                <xsl:if test="@failed > 0"><span class="failure">&#x2718;</span></xsl:if>
                <xsl:if test="@failed = 0"><span class="success">&#x2714;</span></xsl:if>
                &#160;<xsl:value-of select="@name"/>
                &#160;<span class="testcount">(<xsl:value-of select="@total"/>&#160;test<xsl:if test="@total > 1">s</xsl:if>)</span>
            </span>
            <br clear="all" />
        </h3>
        <div class="indent">
            <xsl:if test="@failed = 0"><xsl:attribute name="style">display: none;</xsl:attribute></xsl:if>
            <xsl:attribute name="id">collection<xsl:value-of select="generate-id()"/></xsl:attribute>


    <xsl:for-each select="test">
       <div class="indent">
        <xsl:if test="@result ='Fail'">
            <a>
                <xsl:attribute name="onclick">EnsureBlock('test<xsl:value-of select="generate-id()"/>')</xsl:attribute>
                <xsl:attribute name="href">#test<xsl:value-of select="generate-id()"/></xsl:attribute>
                <span class="failure">&#x2718;</span>
                <xsl:value-of select="@name"/>
            </a>
        </xsl:if>
        <xsl:if test="@result ='Pass'">
            <span class="success">&#x2714;</span>
            <xsl:value-of select="@name"/>
        </xsl:if>
     </div>
    </xsl:for-each>
           
        </div>
    </xsl:template>

</xsl:stylesheet>

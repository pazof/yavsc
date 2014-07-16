<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<p>
     <% var ts = ((string[,])ViewData["Thanks"]);
     if (ts != null) { %>
   <hr/><i> Powered by</i><br/>
     <% for (int i = 0; i <= ts.GetUpperBound(0); i++)
	{

     %>
      <%= "<a href=\""+Html.Encode(ts[i,1])+"\" class=\"socle\">"
      + ts[i,0]+"</a>" 
      %>
     <%
     }}
     %>
</p>				
				

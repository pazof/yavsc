<%@ Page Title="Blogs - Indexe" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master" EnableTheming="True" StylesheetTheme="dark" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<div>
<table>
<% foreach (var g in Model.GroupByUser()) { %>
<tr><th><%= Html.ActionLink(g.Key, "Index",
 new { user = g.Key }, new { @class = "userref" } ) %>
 </th></tr>
<% foreach (var p in g) { %> 
<tr>
<td> 

<%= Html.ActionLink(p.Title, "UserPost",
 new { user = g.Key, title = p.Title }, new { @class = "usertitleref" } ) %>
le  <%=p.Posted.ToString("D") %>
 </td>
  <% if (Membership.GetUser()!=null)
	if ((Membership.GetUser().UserName==g.Key)
	|| (Roles.IsUserInRole ("Admin")))
	 { %><td>
	 <%= Html.ActionLink("Editer","Edit", new { id = p.Id }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","RemovePost", new { id = p.Id }, new { @class="actionlink" } ) %>
</td><% } %>
 </tr> <% } %>
 <% } %>
 </table>
</div>

	  <form runat="server" id="form1" method="GET">
	<% rp1.ResultCount = Model.Count; rp1.ResultsPerPage = 50; %>
<% rp1.CurrentPage = (int) ViewData["PageIndex"]; %>
	 <yavsc:ResultPages id="rp1" Action = "?pageIndex={0}" runat="server" ></yavsc:ResultPages> 
 		
	 </form>
</asp:Content>


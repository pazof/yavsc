<%@ Page Title="Billets utilisateurs" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = ((string) ViewData["BlogTitle"])+" - "+YavscHelpers.SiteName ; %>
</asp:Content>
		<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
			<h1 class="blogtitle"><a href="/Blog/<%=ViewData["BlogUser"]%>">
	<img class="avatar" src="/Blogs/Avatar?user=<%=ViewData["BlogUser"]%>" alt="ViewData["BlogUser"]"/>
	<%=ViewData["BlogTitle"]%></a> - <a href="/"> <%= YavscHelpers.SiteName %> </a> </h1>

</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%  
	foreach (BlogEntry e in this.Model) { %>
	<div <% if (!e.Visible) { %> style="background-color:#022;" <% } %>>

<h2 class="blogtitle" ><%= Html.ActionLink(e.Title,"UserPost", new { user=e.UserName, title = e.Title }) %></h2>
<div class="metablog">(<%= e.Posted.ToString("yyyy/MM/dd") %>
	 - <%= e.Modified.ToString("yyyy/MM/dd") %> <%= e.Visible? "":", Invisible!" %>)
</div>

</div>
<% } %>
	
	 <form runat="server" id="form1" method="GET">
	<%
 rp1.ResultCount = (int) ViewData["RecordCount"];
 rp1.CurrentPage = (int) ViewData["PageIndex"];
 user.Value = (string) ViewData["BlogUser"];

%>

	 <yavsc:ResultPages id="rp1" Action = "?pageIndex={0}" runat="server"></yavsc:ResultPages> 
 		<asp:HiddenField id="user" runat="server"></asp:HiddenField>
	 </form>
	

</asp:Content>
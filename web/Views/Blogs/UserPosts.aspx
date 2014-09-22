<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master"%>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ContentPlaceHolderID="titleContent" ID="titleContent" runat="server"><%=Html.Encode(ViewData["BlogTitle"])%></asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
	<h1><a href="/Blog/<%=ViewData["BlogUser"]%>">
	<img class="avatar" src="/Blogs/Avatar?user=<%=ViewData["BlogUser"]%>" alt=""/>
	<%=ViewData["BlogTitle"]%></a></h1>
		<div class="message"><%= Html.Encode(ViewData["Message"])%></div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%  
	foreach (BlogEntry e in this.Model) { %>
	<div <% if (!e.Visible) { %> style="background-color:#022;" <% } %>>

<h2 class="blogtitle" ><%= Html.ActionLink(e.Title,"UserPost", new { user=e.UserName, title = e.Title }) %></h2>
<div class="metablog">(<%= e.Posted.ToString("yyyy/MM/dd") %>
	 - <%= e.Modified.ToString("yyyy/MM/dd") %> <%= e.Visible? "":", Invisible!" %>)
</div>
<% if (Membership.GetUser()!=null)
	if (Membership.GetUser().UserName==e.UserName)
	 { %>
	 <%= Html.ActionLink("Editer","Edit", new { user = e.UserName, title = e.Title }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","Remove", new { user = e.UserName, title = e.Title }, new { @class="actionlink" } ) %>
	 <% } %>
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
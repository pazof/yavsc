<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEditEntryModel>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Model.Title+" (édition) - "+ViewData["BlogTitle"]; %>
</asp:Content>
<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1 class="blogtitle"><%= Html.ActionLink(Model.Title,"UserPost",new{user=Model.UserName,title=Model.Title}) %> (édition) - 
<a href="/Blog/<%=Model.UserName%>"><img class="avatar" src="/Blogs/Avatar?user=<%=Model.UserName%>" alt=""/> <%=ViewData["BlogTitle"]%></a>
 <asp:Literal runat="server" Text=" - " />
   <a href="/"> <%= YavscHelpers.SiteName %> </a> </h1>
 
<div class="metablog">(Id:<a href="/Blogs/UserPost/<%=Model.Id%>"><i><%=Model.Id%></i></a>, <%= Model.Posted.ToString("yyyy/MM/dd") %>
	 - <%= Model.Modified.ToString("yyyy/MM/dd") %> <%= Model.Visible? "":", Invisible!" %>)
<%  if (Membership.GetUser()!=null)
	if (Membership.GetUser().UserName==Model.UserName)
	 {  %>
	 <%= Html.ActionLink("Editer","Edit", new { user=Model.UserName, title = Model.Title }, new { @class="actionlink" }) %>
	 <%= Html.ActionLink("Supprimer","RemovePost", new { user=Model.UserName, title = Model.Title }, new { @class="actionlink" } ) %>
	 <% } %>
	 </div>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
	 <% if (Model != null ) if (Model.Content != null )  {
	 BBCodeHelper.InitParser (); %>
	 <%= Html.ActionLink(Model.Title,"UserPost",new{user=Model.UserName,title=Model.Title}) %>
<div class="blogpost">
<%= BBCodeHelper.Parser.ToHtml(Model.Content) %>
</div>
<% } %>
Usage BBcodes  :
	 <div style="font-family:monospace;">
<%
	foreach (string usage in BBCodeHelper.BBTagsUsage) { %>
<div style="display:inline"><%= usage %></div>
<% } %>
	</div>

<%= Html.ValidationSummary("Edition du billet") %>

<% using(Html.BeginForm("ValidateEdit","Blogs")) { %>
<%= Html.LabelFor(model => model.Title) %>:<br/>
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<br/>
<%= Html.LabelFor(model => model.Content) %>:<br/>
<%= Html.TextArea( "Content" , new { @class="editblog", @rows="15" }) %>
<%= Html.ValidationMessage("Content", "*") %>
<br/>
<%= Html.CheckBox( "Visible" ) %>
<%= Html.LabelFor(model => model.Visible) %>
<%= Html.ValidationMessage("Visible", "*") %>
	<br/>
<%= Html.CheckBox( "Preview" ) %>
<%= Html.LabelFor(model => model.Preview) %>
<%= Html.ValidationMessage("Preview", "*") %>
	<%= Html.Hidden("Id") %>
	<%= Html.Hidden("UserName") %>

<br/>
<input type="submit"/>
<% } %>
</div>

</asp:Content>


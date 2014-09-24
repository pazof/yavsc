<%@ Page Title="Billet" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Model.Title+" - "+ViewData["BlogTitle"]; %>
</asp:Content>
<asp:Content ContentPlaceHolderID="overHeaderOne" ID="header1" runat="server">
<h1 class="blogtitle"><%= Html.ActionLink(Model.Title,"UserPost",new{user=Model.UserName,title=Model.Title}) %> - 
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
	<div>
<div class="blogpost">
	 <% BBCodeHelper.Init(); %>
<%= BBCodeHelper.Parser.ToHtml(Model.Content) %>
</div>
	 <% 
	string username = "";
	if (Membership.GetUser()!=null)
		username = Membership.GetUser().UserName; %>

<% foreach (var c in (Comment[])ViewData["Comments"]) {  %> 
	
<div class="comment" style="min-height:32px;"> <img style="clear:left;float:left;max-width:32px;max-height:32px;margin:.3em;" src="/Blogs/Avatar/<%=c.From%>" alt="<%=c.From%>"/>
<%= BBCodeHelper.Parser.ToHtml(c.CommentText) %>
	<% if ( username == Model.UserName || c.From == username ) { %>
	<%= Html.ActionLink("Supprimer","RemoveComment", new { cmtid = c.Id } )%>
	<% } %>
</div>
<% } %>
	 <div class="postcomment">
	 <% using (Html.BeginForm("Comment","Blogs")) { %>
	 <%=Html.Hidden("UserName")%>
	 <%=Html.Hidden("Title")%>
	 <%=Html.TextArea("CommentText","")%>
	 <%=Html.Hidden("PostId",Model.Id)%>
	 <input type="submit" value="Poster un commentaire"/>
	 <% } %>
	  </div>
</div>
</asp:Content>

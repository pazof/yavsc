<%@ Page Title="Yavsc - indexe" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<p>
« Voir le monde comme un rêve est un bon point de vue. Quand on fait un cauchemar, on se réveille et on se dit que ce n’était qu’un rêve. Il est dit que le monde où nous vivons n’en diffère en rien ». 
<br/><a href="http://unefenetresurlemonde.over-blog.com/article-34325590.html">Ghost Dog, la Voie du samouraï</a>
</p><div>
<%= Html.ActionLink("Les blogs","Index","Blogs") %>
</div>
 </asp:Content>


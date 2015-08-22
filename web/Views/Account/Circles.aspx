<%@ Page Title="Circles" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Register Assembly="Yavsc.WebControls" TagPrefix="yavsc" Namespace="Yavsc.WebControls" %> 
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<script type="text/javascript" src="<%=Url.Content("~/Scripts/stupidtable.js")%>"></script> 
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<table  id="tbc">
<thead>
<tr>
<th data-sort="string"><%=Html.Translate("Title")%>
</th>
</tr>
</thead>
<tbody id="tbcb">
<% int lc=0;
   foreach (var ci in (IEnumerable<Circle>) ViewData["Circles"]) { lc++; %>
<tr class="<%= (lc%2==0)?"even ":"odd " %>row" id="c_<%=ci.Id%>">
<td cid="<%=ci.Id%>" style="cursor: pointer;" class="btnselcircle" ><%=Html.FormatCircle(ci)%></td>
   <td>
   <input type="button" value="<%=Html.Translate("Remove")%>" 
   class="btnremovecircle actionlink" cid="<%=ci.Id%>"/>
    </td>
</tr>
<% } %>
</tbody>
</table>

<div class="actionlink" id="btednvcirc" did="fncirc">Ajouter un cercle</div>
<script>
$(function(){
$("#tbc").stupidtable();
});
</script>
</asp:Content>
<asp:Content ID="MASContentContent" ContentPlaceHolderID="MASContent" runat="server">
<div id="fncirc" class="hidden">


<div class="panel">
<form>
<fieldset>
<legend id="lgdnvcirc"></legend>
<span id="msg" class="field-validation-valid error"></span>
<label for="title"><b><%=Html.Translate("Title")%></b></label>
<input type="text" id="title" name="title" class="inputtext"/>
<span id="Err_cr_title" class="field-validation-valid error"></span>
<table  id="tbmbrs">
<thead>
<tr>
<th data-sort="string"><%=Html.Translate("Members")%>
<span id="Err_cr_users" class="field-validation-valid error"></span>
	 <yavsc:InputUserName
	  id="users"
	  name="users"
	   emptyvalue="[aucun]"
	 onchange="onmembersChange(this.value);"
	 multiple="true"
	   runat="server" >
	 </yavsc:InputUserName>

</th>
</tr>
</thead>
<tbody id="tbmbrsb">
</tbody>
</table>
<input type="button" id="btnnewcircle" value="<%=Html.Translate("Create")%>" class="actionlink rowbtnct" />
</fieldset>
</form>



</div>


</div>


<script>

var cformhidden=true;
var errspanid="msg";

function getCircle(id)
{
$.getJSON("<%=Url.Content("~/api/Circle/Get/")%>"+id,
function(json) { $("#title").val( json.Title); $("#users").val( json.Members ) ; });
}

function editNewCircle() {
  if ($('#fncirc').hasClass('hidden')) $('#fncirc').removeClass('hidden')
	$('#lgdnvcirc').html("Creation d'un cercle");
 }

function selectCircle() {
    if ($('#fncirc').hasClass('hidden')) $('#fncirc').removeClass('hidden')
	var id = $(this).attr('cid');
	$('#lgdnvcirc').html("Edition du cercle");
    // get it from the server
    getCircle(id);
}

 	function onmembersChange(newval)
 	{
 	}

    function removeCircle() {
    	message(false);
    	 var id = $(this).attr('cid'); 
    	 $.ajax({
            url: "<%=Url.Content("~/api/Circle/Delete/")%>"+id,
            type: "GET",
            success: function (data) { 
            // Drops the row
            $("#c_"+id).remove();
           },
            statusCode: {
            	400: function(data) {
            		$.each(data.responseJSON, function (key, value) {
            		var errspanid = "Err_cr_" + value.key.replace("model.","");
            		var errspan = document.getElementById(errspanid);
            		if (errspan==null)
            			alert('enoent '+errspanid);
            		else 
            			errspan.innerHTML=value.errors.join("<br/>");
                	});
            		}
            	},
            error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}});
    }

    function addCircle()
    {
    message(false);
  	  var circle = { title: $("#title").val(), users: $("#users").val() } ;
    	 $("#title").text('');
    	 $("#users").val('');
    	 $.ajax({
            url: "<%=Url.Content("~/api/Circle/Create")%>",
            type: "POST",
            data: circle,
            success: function (id) { 
            // Adds a node rendering the new circle
            $('<tr id="c_'+id+'"/>').addClass('selected row')
            .appendTo('#tbcb');

            $('<td>'+circle.title+'<br><i>'+
            circle.users+
            '</i></td></td>')
            .appendTo('#c_'+id);

            $('<td><input class="btnremovecircle actionlink" cid="'+id+'" type="button" value="Remove" onclick="removeCircle"></td>').appendTo('#c_'+id);
         
           },
            statusCode: {
            	400: function(data) {
            		$.each(data.responseJSON, function (key, value) {
            		var errspanid = "Err_cr_" + value.key.replace("model.","");
            		var errspan = document.getElementById(errspanid);
            		if (errspan==null)
            			alert('enoent '+errspanid);
            		else 
            			errspan.innerHTML=value.errors.join("<br/>");
                	});
            		}
            	},
            error: function (xhr, ajaxOptions, thrownError) {
            	if (xhr.status!=400)
        			message(xhr.status+" : "+xhr.responseText);
			    else message(false);
        		}});
    }
 $(document).ready(function () {
 $('#btednvcirc').click(editNewCircle);
 $("#btnnewcircle").click(addCircle);
 $(".btnremovecircle").click(removeCircle);
 $(".btnselcircle").click(selectCircle);
    });
 	</script>
</asp:Content>

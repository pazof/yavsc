

var errspanid="msg";
var CirclesApiUrl = apiBaseUrl + "/Circle";



function editNewCircle() {
  if ($('#fncirc').hasClass('hidden')) $('#fncirc').removeClass('hidden')
	$('#lgdnvcirc').html("Creation d'un cercle");
	$('#fncirc').removeClass("dirty");
	$("#title").val( '' ); 
	$('#btnnewcircle').show();
	$('#btneditcircle').hide();
 }

function selectCircle() {
    if ($('#fncirc').hasClass('hidden')) $('#fncirc').removeClass('hidden')
	var id = $(this).attr('cid');
	$('#lgdnvcirc').html("Edition du cercle");
	$('#btnnewcircle').hide();
	$('#btneditcircle').show();
    // get it from the server

    $.getJSON(CirclesApiUrl+"/Get/"+id, function(json) { 
		$("#title").val( json.Title); });
	$('#id').val(id);
	$('#fncirc').removeClass("dirty");
}

function onCircleChanged() 
    { $('#fncirc').addClass("dirty"); }

function removeCircle() {
    	Yavsc.message(false);
    	 var id = $(this).attr('cid'); 
    	 $.ajax({
            url: CirclesApiUrl+"/Delete/"+id,
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
        			Yavsc.message(xhr.status+" : "+xhr.responseText);
			    else Yavsc.message(false);
        		}});
    }
 function modifyCircle() {
 Yavsc.message(false);
 var circle = { title: $("#title").val(), id: $('#id').val(), isprivate: $('#isprivate').val() } ;
 }
    
function addCircle()
    {
    Yavsc.message(false);
  	  var circle = { title: $("#title").val() } ;
    	 $("#title").text('');
    	 $.ajax({
            url: CirclesApiUrl+"/Create",
            type: "POST",
            data: circle,
            success: function (id) { 
            // Adds a node rendering the new circle
            $('<tr id="c_'+id+'"/>').addClass('selected row')
            .appendTo('#tbcb');

            $('<td>'+circle.title+' <br><i>'+
            circle.members+
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
        			Yavsc.message(xhr.status+" : "+xhr.responseText);
			    else Yavsc.message(false);
        		}});
    }

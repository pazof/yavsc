+(function ($) {
  var pvuis

  var audio = new Audio('/sounds/bell.mp3')

  function addULI (uname, cxids) {
    $('<li class="user"><button><img src="/Avatars/' + uname + '.xs.png"> ' + uname + '</button></li>')
      .data('name', uname)
      .data('cxids', cxids)
      .css('cursor', 'pointer')
      .click(function () { setPrivate(this); })
      .appendTo('#userlist')
  }

  function getUsers () {
    $('#userlist').empty()
    $('#to').empty()
    $.get('/api/chat/users').done(
      function (users) {
        $.each(users, function () {
          var user = this
          var existent = $('#userlist li').filterByData('name', user.UserName)
          if (existent.length > 0) existent.remove()
          var cxids = []
          $.each(user.Connections, function () {
            cxids.push(this.ConnectionId)
          })
          addULI(user.UserName, cxids)
        })
      }
    )
  }

  function onCx () {
    setTimeout(function () {
      getUsers()
    }, 120),
    $('#chatview').removeClass('disabled')
    $('#sendmessage').prop('disabled',false);
    $('#sendpv').prop('disabled',false);
  }
  function onDisCx () {
    $('#chatview').addClass('disabled');
    $('#sendmessage').prop('disabled',true);
    $('#sendpv').prop('disabled',true);

  }
  // This optional function html-encodes messages for display in the page.
  function htmlEncode (value) {
    var encodedValue = $('<div />').text(value).html()
    return encodedValue
  }

  var setPrivate = function (li) {
    $('#sendmessagebox').addClass('hidden')
    $('#sendpvbox').removeClass('hidden')
    pvuis = { CXs: $(li).data('cxids'), UserName: $(li).data('name') }
    $('#sendpvdest').html(pvuis.UserName)
    $('#pv').focus()
  }
  var setPublic = function () {
    $('#sendmessagebox').removeClass('hidden')
    $('#sendpvbox').addClass('hidden')
    $('#message').focus()
  }
  $('#pubChan').css('cursor', 'pointer')
  $('#pubChan').click(setPublic)
  setPublic()
  // Reference the auto-generated proxy for the hub.
  var chat = $.connection.chatHub
  // Create a function that the hub can call back to display messages.
  chat.client.addMessage = function (name, message) {
    // Add the message to the page.
    $('#discussion').append('<li class="discussion"><strong>' + htmlEncode(name)
      + '</strong>: ' + htmlEncode(message) + '</li>')
  }
  chat.client.addPV = function (name, message) {
    if (!$('#mute').prop('checked')) {
      audio.play()
    }
    // Add the pv to the page.
    $('#discussion').append('<li class="pv"><strong>' + htmlEncode(name)
      + '</strong>: ' + htmlEncode(message) + '</li>')
  }
  $.fn.filterByData = function (prop, val) {
    return this.filter(
      function () { return $(this).data(prop) == val; }
    )
  }

  var onUserDisconnected = function (cxid) {
    $('#userlist li').filter(function () {
      var nids = $(this).data('cxids').filter(function () {
        return $(this) !== cxid
      })
      if (nids.Length == 0) $(this).remove()
      else $(this).data('cxids', nids)
    })
  }
  var onUserConnected = function (cxid, username) {
    var connected = $('#userlist li').filterByData('name', username)
    if (connected.length > 0) {
      var ids = connected.data('cxids')
      ids.push(cxid)
      connected.data('cxids', ids)
    } else {
      addULI(username, [cxid])
    }
  }
  chat.client.notify = function (tag, message, data) {
    if (data) {
      // Add the pv to the page.

      if (tag === 'connected') {
        onUserConnected(message, data)
        $('#discussion').append('<li class="notif"><i>' + htmlEncode(tag)
          + '</i> ' + htmlEncode(data) + '</li>')
      }
      else if (tag === 'disconnected') {
        onUserDisconnected(message, data)
        $('#discussion').append('<li class="notif"><i>' + htmlEncode(tag)
          + '</i> ' + htmlEncode(data) + '</li>')
      }else {
        $('#discussion').append('<li class="notif"><i>' + htmlEncode(tag)
          + '</i> ' + htmlEncode(message) + ' : ' + htmlEncode(data) + '</li>')
      }
    }
  }

  var sendMessage = function () {
    chat.server.send($('#displayname').val(), $('#message').val())
    // Clear text box and reset focus for next comment.
    $('#message').val('')
  }

  var sendPV = function () {
    var msg = $('#pv').val()
    // Call the Send method on the hub.
    $.each(pvuis.CXs, function () {
      chat.server.sendPV(this, msg)
    })
    $('#discussion').append('<li class="pv">' + htmlEncode(pvuis.UserName)
      + '<< ' + htmlEncode(msg) + '</li>')
    // Clear text box and reset focus for next comment.
    $('#pv').val('')
  }

  // Start the connection.
  $.connection.hub.start().done(function () {
    onCx()
    $('#sendmessage').click(function () {
      // Call the Send method on the hub.
      sendMessage()
      $('#message').focus()
    })
    $('#message').keydown(function (event) {
      if (event.which == 13) {
        sendMessage()
      }
    })
    $('#pv').keydown(function (event) {
      if (event.which == 13) {
        sendPV()
      }
    })
    $('#sendpv').click(function () {
      // Call the Send method on the hub.
      sendPV()
      $('#sendpv').focus()
    })
  })

  $.connection.hub.disconnected(function () {
    onDisCx()
    setTimeout(function () {
      $.connection.hub.start().done(function () {
        $('#mySignalRConnectionIdHidden').val($.connection.hub.id)
        onCx()
      }, 30000); // Re-start connection after 30 seconds
    })
  })

  $(window).unload(function () { chat.server.abort(); })
})(jQuery);
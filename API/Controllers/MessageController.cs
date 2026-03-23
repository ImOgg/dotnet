using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/messages")]
public class MessageController(IMessageRepository messageRepository, IMemberRepository memberRepository) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
    {
        var sender = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
        var recipient = await memberRepository.GetMemberByIdAsync(createMessageDTO.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDTO.RecipientId)
            return BadRequest("Cannot sent this message");
        var message = new Message
        {
            SenderId = sender.Id,
            Sender = sender,
            RecipientId = recipient.Id,
            Recipient = recipient,
            Content = createMessageDTO.Content
        };
        messageRepository.AddMessage(message);
        if (await messageRepository.SaveAllAsync()) return message.ToDto();
        return BadRequest("Failed to send message");
    }
}

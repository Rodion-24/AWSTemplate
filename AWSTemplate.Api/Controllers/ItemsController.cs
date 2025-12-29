using AWSTemplate.Application.Items.Commands;
using AWSTemplate.Application.Items.DTOs;
using AWSTemplate.Application.Items.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AWSTemplate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ItemDto>>> GetAll()
    {
        var items = await _mediator.Send(new GetAllItemsQuery());
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDto>> GetById(Guid id)
    {
        var item = await _mediator.Send(new GetItemByIdQuery(id));
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemCommand command)
    {
        var created = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ItemDto>> Update(Guid id, [FromBody] UpdateItemCommand command)
    {
        if (id != command.Id) return BadRequest("Id mismatch");

        var updated = await _mediator.Send(command);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _mediator.Send(new DeleteItemCommand(id));
        return deleted ? NoContent() : NotFound();
    }
}

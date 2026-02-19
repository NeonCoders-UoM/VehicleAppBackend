using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleTransferController : ControllerBase
    {
        private readonly IVehicleTransferService _transferService;

        public VehicleTransferController(IVehicleTransferService transferService)
        {
            _transferService = transferService;
        }

        /// <summary>
        /// Initiate a vehicle transfer to another user
        /// </summary>
        [HttpPost("initiate")]
        public async Task<ActionResult<TransferResponseDTO>> InitiateTransfer([FromBody] InitiateTransferDTO dto, [FromQuery] int sellerId)
        {
            var result = await _transferService.InitiateTransfer(dto, sellerId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message, transfer = result.Transfer });
        }

        /// <summary>
        /// Accept a pending vehicle transfer
        /// </summary>
        [HttpPost("accept/{transferId}")]
        public async Task<ActionResult<TransferResponseDTO>> AcceptTransfer(int transferId, [FromQuery] int buyerId)
        {
            var result = await _transferService.AcceptTransfer(transferId, buyerId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message, transfer = result.Transfer });
        }

        /// <summary>
        /// Reject a pending vehicle transfer
        /// </summary>
        [HttpPost("reject/{transferId}")]
        public async Task<ActionResult> RejectTransfer(int transferId, [FromQuery] int buyerId)
        {
            var result = await _transferService.RejectTransfer(transferId, buyerId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Cancel a pending vehicle transfer (seller only)
        /// </summary>
        [HttpPost("cancel/{transferId}")]
        public async Task<ActionResult> CancelTransfer(int transferId, [FromQuery] int sellerId)
        {
            var result = await _transferService.CancelTransfer(transferId, sellerId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Get all pending transfers for a buyer
        /// </summary>
        [HttpGet("pending/{customerId}")]
        public async Task<ActionResult<List<TransferResponseDTO>>> GetPendingTransfers(int customerId)
        {
            var transfers = await _transferService.GetPendingTransfersForBuyer(customerId);
            return Ok(transfers);
        }

        /// <summary>
        /// Get transfer history for a vehicle
        /// </summary>
        [HttpGet("history/{vehicleId}")]
        public async Task<ActionResult<List<TransferResponseDTO>>> GetTransferHistory(int vehicleId)
        {
            var transfers = await _transferService.GetTransferHistory(vehicleId);
            return Ok(transfers);
        }
    }
}

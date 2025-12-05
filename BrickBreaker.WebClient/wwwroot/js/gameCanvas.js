let canvasEl;
let ctx;

export function init(canvas, width, height) {
  canvasEl = canvas;
  ctx = canvas.getContext("2d");
  canvasEl.width = width;
  canvasEl.height = height;
  ctx.textBaseline = "middle";
  ctx.font = "14px 'Consolas', monospace";
}

export function render(state) {
  if (!ctx || !canvasEl) {
    return;
  }

  const {
    playArea,
    paddle,
    balls,
    bricks,
    powerUps,
    scorePopups,
    overlay
  } = state;

  ctx.clearRect(0, 0, canvasEl.width, canvasEl.height);
  ctx.fillStyle = "#03030a";
  ctx.fillRect(0, 0, canvasEl.width, canvasEl.height);

  drawPlayArea(playArea, overlay);
  drawBricks(bricks);
  drawPowerUps(powerUps);
  drawBalls(balls);
  drawPaddle(paddle, overlay);
  drawPopups(scorePopups);
  drawOverlay(playArea, overlay);
}

function drawPlayArea(area, overlay) {
  ctx.save();
  ctx.lineWidth = 6;
  ctx.strokeStyle = `hsl(${overlay.borderHue}, 100%, 60%)`;
  ctx.strokeRect(area.x, area.y, area.width, area.height);
  ctx.fillStyle = "#11142a";
  ctx.fillRect(area.x, area.y, area.width, area.height);
  ctx.restore();
}

function drawBricks(bricks) {
  bricks.forEach(brick => {
    ctx.fillStyle = `rgb(${brick.color.r}, ${brick.color.g}, ${brick.color.b})`;
    ctx.fillRect(brick.x, brick.y, brick.width, brick.height);
    ctx.strokeStyle = "rgba(0,0,0,0.55)";
    ctx.strokeRect(brick.x, brick.y, brick.width, brick.height);
  });
}

function drawPowerUps(powerUps) {
  ctx.save();
  ctx.lineWidth = 2;
  powerUps.forEach(powerUp => {
    const centerX = powerUp.x + powerUp.width / 2;
    const centerY = powerUp.y + powerUp.height / 2;
    const isMultiball = powerUp.kind === "Multiball";

    ctx.beginPath();
    ctx.fillStyle = isMultiball ? "#ffe066" : "#8ef5ff";
    ctx.ellipse(centerX, centerY, powerUp.width / 2, powerUp.height / 2, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.strokeStyle = "#03030a";
    ctx.stroke();

    ctx.fillStyle = "#050308";
    ctx.font = "bold 16px 'Consolas', monospace";
    ctx.textAlign = "center";
    ctx.fillText(isMultiball ? "M" : "E", centerX, centerY + 1);
  });
  ctx.restore();
}

function drawBalls(balls) {
  ctx.save();
  ctx.strokeStyle = "#ffffff";
  ctx.lineWidth = 2;
  ctx.fillStyle = "#ff4d6d";
  balls.forEach(ball => {
    ctx.beginPath();
    ctx.arc(ball.x + ball.radius, ball.y + ball.radius, ball.radius, 0, Math.PI * 2);
    ctx.fill();
    ctx.stroke();
  });
  ctx.restore();
}

function drawPaddle(paddle, overlay) {
  ctx.save();
  ctx.fillStyle = overlay.ballReady ? "#2ec4ff" : "#3b9dff";
  ctx.fillRect(paddle.x, paddle.y, paddle.width, paddle.height);
  ctx.strokeStyle = "#081427";
  ctx.strokeRect(paddle.x, paddle.y, paddle.width, paddle.height);
  ctx.restore();
}

function drawPopups(popups) {
  ctx.save();
  ctx.font = "bold 16px 'Consolas', monospace";
  popups.forEach(popup => {
    ctx.globalAlpha = popup.opacity;
    ctx.fillStyle = popup.isMultiplier ? "#ff8d57" : "#ffe169";
    ctx.fillText(popup.text, popup.x, popup.y);
  });
  ctx.restore();
  ctx.globalAlpha = 1;
}

function drawOverlay(playArea, overlay) {
  if (!overlay.ballReady && !overlay.isPaused && !overlay.isGameOver) {
    return;
  }

  ctx.save();
  ctx.textAlign = "center";
  ctx.font = "bold 18px 'Consolas', monospace";
  const centerX = playArea.x + playArea.width / 2;
  const centerY = playArea.y + playArea.height / 2;

  if (overlay.isGameOver) {
    ctx.fillStyle = "rgba(0, 0, 0, 0.65)";
    ctx.fillRect(playArea.x, playArea.y, playArea.width, playArea.height);
    ctx.fillStyle = "#ff6b6b";
    ctx.fillText("Game Over", centerX, centerY - 10);
    ctx.fillStyle = "#f5f7ff";
    ctx.font = "14px 'Consolas', monospace";
    ctx.fillText("Press Space to try again", centerX, centerY + 12);
  } else if (overlay.isPaused) {
    ctx.fillStyle = "rgba(12, 16, 31, 0.85)";
    ctx.fillRect(playArea.x, playArea.y, playArea.width, playArea.height);
    ctx.fillStyle = "#f5f7ff";
    ctx.fillText("Paused", centerX, centerY);
  } else if (overlay.ballReady) {
    ctx.fillStyle = "#f5f7ff";
    ctx.fillText("Press ↑ or W to launch", centerX, centerY);
  }

  ctx.restore();
}

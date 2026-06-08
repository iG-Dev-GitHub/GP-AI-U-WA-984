"""One-time asset generation for Workout Drop Plinko Gym via Gemini Nano Banana.
Saves PNGs with transparent backgrounds to /app/frontend/assets/images/plinko/.
"""
import asyncio
import base64
import os
from pathlib import Path
from dotenv import load_dotenv
from emergentintegrations.llm.chat import LlmChat, UserMessage

load_dotenv(Path(__file__).parent / ".env")
API_KEY = os.getenv("EMERGENT_LLM_KEY")
OUT = Path("/app/frontend/assets/images/plinko")
OUT.mkdir(parents=True, exist_ok=True)

ASSETS = {
    "kettlebell": (
        "A single steel kettlebell weight, dark grey metallic with subtle highlights, "
        "cartoon flat art style, thick black outline, bold simple shapes, centered, "
        "PNG with fully transparent background, no shadow on ground, square 1024x1024, "
        "viewed from front, gym equipment icon."
    ),
    "kettlebell_fire": (
        "A steel kettlebell weight engulfed in bright orange and red flames with "
        "a fiery trail behind it, cartoon flat art style, thick black outline, "
        "neon glow, dynamic motion, centered, PNG with fully transparent background, "
        "square 1024x1024, beast mode HIIT energy."
    ),
    "badge_full_drop": (
        "A round golden trophy badge with a plinko ball icon in the center and the "
        "text 'FULL DROP' in bold cartoon font around the rim, gold metallic with "
        "neon cyan glow, flat cartoon art, thick black outline, PNG transparent "
        "background, square 1024x1024."
    ),
    "badge_beast_mode": (
        "A round badge with a fierce flaming skull and crossed barbells, bright red "
        "and orange fire colors, cartoon flat art with thick black outline, "
        "the text 'BEAST MODE' in bold caps on a banner, PNG transparent background, "
        "square 1024x1024."
    ),
    "badge_iron_week": (
        "A round metallic iron-grey badge shaped like a shield with a number 5 and "
        "a calendar icon, bold cartoon flat art, thick black outline, neon blue glow, "
        "the text 'IRON WEEK' on a banner, PNG transparent background, square 1024x1024."
    ),
    "badge_pr": (
        "A round golden star badge with a barbell icon in the center, sparkles around "
        "it, gold metallic with bright yellow neon glow, cartoon flat art, thick black "
        "outline, the letters 'PR' in bold, PNG transparent background, square 1024x1024."
    ),
}


async def gen(name: str, prompt: str) -> None:
    out_path = OUT / f"{name}.png"
    if out_path.exists():
        print(f"SKIP {name} (exists)")
        return
    print(f"GEN  {name} ...")
    chat = LlmChat(
        api_key=API_KEY,
        session_id=f"asset-{name}",
        system_message="You are an expert digital illustrator for mobile games.",
    ).with_model("gemini", "gemini-3.1-flash-image-preview").with_params(
        modalities=["image", "text"]
    )
    msg = UserMessage(text=prompt)
    _text, images = await chat.send_message_multimodal_response(msg)
    if not images:
        print(f"FAIL {name}: no image returned")
        return
    img = images[0]
    out_path.write_bytes(base64.b64decode(img["data"]))
    print(f"OK   {name} -> {out_path}")


async def main() -> None:
    for name, prompt in ASSETS.items():
        try:
            await gen(name, prompt)
        except Exception as e:
            print(f"ERR  {name}: {e}")


if __name__ == "__main__":
    asyncio.run(main())

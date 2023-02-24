from guessit import api
from fastapi import FastAPI

app = FastAPI()


@app.get("/")
def read_root(filename: str):
    return api.guessit(filename)

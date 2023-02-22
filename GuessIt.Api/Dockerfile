FROM python:3.7-alpine
WORKDIR /app
COPY requirements.txt main.py ./
RUN pip install -r requirements.txt && pip install uvicorn
ENTRYPOINT ["uvicorn", "main:app", "--host", "0.0.0.0"]

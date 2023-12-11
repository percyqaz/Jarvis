import speech_recognition as sr

# Initialize the recognizer
recognizer = sr.Recognizer()

def process_command(command):
    # Add your logic here to process the detected command
    print("Command:", command)

def listen_to_microphone():
    with sr.Microphone() as source:
        print("Listening...")
        try:
            audio = recognizer.listen(source)
            recognized_text = recognizer.recognize_google(audio)
            process_command(recognized_text)
        except sr.UnknownValueError:
            print("Could not understand audio")
        except sr.RequestError as e:
            print("Error with the request; {0}".format(e))

if __name__ == "__main__":
    while True:
        listen_to_microphone()

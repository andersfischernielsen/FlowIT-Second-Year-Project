# Feedback from Eduardo

- Didn't see the program. 



The description of the workflow describes appointments as well as exams, can the difference between these be described further? As it is now, the difference is hard to see.

**Is it correctly understood, that the hospital’s responsibility starts as our graph is represented?**  YAS


**Feedback on the process-model:**

Patient check-in: Patient can not be checked in several times. 

Need a response-relation between PreparePatient<->MedicalExamination<->MedicalReport<->Prescription


- Appointment and Examination are two distinct things. 
- Have two distinct queues. 


After medical report, specialist may
- request some exams
- recommend another appointment
- recommend another specialist (or both)
- return
- recommend surgery


Who approves? Employer of Brazilian Government. In case, it does not approve, 


If patient has to return to hospital (if he has a treatment), he may return to hospital multiple times. 
Every time a new schedule is made, the process model has to allow for a patient to go over again. 


Questions in report:

- Preparation of patient: We may assume a nurse is responsible for preparing a patient
- Five options: (Assume each one excludes)
- A specialist can only recommend according to his specialty. 
- ROTA: Only specialists can recommend patient to ROTA. 
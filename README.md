# **About Shakkil:**

SHAKKIL a system that would be able to diacritize Arabic texts automatically. In this system, the diacritization problem is handled through two levels; morphological and syntactic processing levels. The adopted morphological disambiguation algorithm depends on four layers:\
**1)** Uni-morphological form layer.\
**2)** rule-based morphological disambiguation layer.\
**3)** statistical-based disambiguation layer.\
**4)** Out Of Vocabulary (OOV) layer.\

The adopted syntactic disambiguation algorithms is concerned with detecting the case ending diacritics depending on a rule based approach simulating the shallow parsing technique. This is achieved using **(MASA)**; a morphologically annotated corpus for extracting the Arabic linguistic rules, building the language models and testing the system output. 

This system is considered as a good trial of the interaction between rule-based approach and statistical approach, where the rules can help the statistics in detecting the right diacritization and vice versa. At this point, the morphological Word Error Rate (WER) is 4.56% while the morphological Diacritic Error Rate (DER) is 1.88% and the syntactic WER is 9.36% and the WER is 14.78%.

# **Citation**

If you use Shakkil for your scientific publication, or if you find the resources in this repository useful, please cite our paper as follows (to be updated):
``
@inproceedings{fashwan2017shakkil,\
  title={SHAKKIL: an automatic diacritization system for modern standard Arabic texts},\
  author={Fashwan, Amany and Alansary, Sameh},\
  booktitle={Proceedings of the Third Arabic Natural Language Processing Workshop},\
  pages={84--93},\
  year={2017}\
}
``

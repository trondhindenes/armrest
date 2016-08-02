#!/usr/bin/env python

import requests
import sys, getopt
from ansible.constants import p, get_config
from ansible import utils

operation = str(sys.argv[1])

#if running multiple armrest instances, use this to separate them
#for instance, if armrest_instance is set to "prod", the corresponding configuration section in ansible.cfg should be "armrest_prod"
armrest_instance = ""

if armrest_instance != "":
	armrest_config = "armrest_" + armrest_instance
else:
	armrest_config = "armrest"

armrest_uri = get_config(p, armrest_config, "armrest_uri", "ARMREST_URI","")
#print(armrest_uri)
armrest_use_cache = get_config(p, armrest_config, "armrest_use_cache", "ARMREST_USE_CACHE","")
armrest_cache_lifetime_seconds = get_config(p, armrest_config, "armrest_cache_lifetime_seconds", "ARMREST_CACHE_LIFETIME_SECONDS","")

if (armrest_use_cache == True):
	import requests_cache
	requests.cache.install_cache('armrest_cache', backend='sqlite', expire_after=armrest_cache_lifetime_seconds)

if (operation == '--list'):
	armrest_list_uri = armrest_uri + "/api/listhosts"
	r = requests.get(armrest_list_uri)
	print(r.text)

if (operation == '--host'):
	armrest_host = str(sys.argv[2])
	armrest_host_uri = armrest_uri + "/api/getsinglehost?hostname=" + armrest_host
	r = requests.get(armrest_host_uri)
	print(r.text)
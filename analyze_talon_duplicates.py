#!/usr/bin/env python3
"""
Talon Commands Duplicate Analysis Script

This script analyzes the TalonCommands.csv file to find:
1. Global commands with the same voice command
2. Commands with the same voice command within the same application
3. Commands with identical combinations of Command+Application
4. Commands with identical combinations of Command+Repository
"""

import pandas as pd
import sys
from collections import defaultdict

def load_talon_data(csv_path):
    """Load the Talon commands CSV file"""
    try:
        df = pd.read_csv(csv_path)
        print(f"Loaded {len(df)} commands from {csv_path}")
        return df
    except Exception as e:
        print(f"Error loading CSV file: {e}")
        return None

def find_global_command_duplicates(df):
    """Find duplicate global commands with the same voice command"""
    print("\n" + "="*60)
    print("GLOBAL COMMAND DUPLICATES")
    print("="*60)
    
    # Filter for global commands
    global_commands = df[df['Application'] == 'global'].copy()
    
    # Group by Command to find duplicates
    command_groups = global_commands.groupby('Command')
    duplicates = []
    
    for command, group in command_groups:
        if len(group) > 1:
            duplicates.append({
                'command': command,
                'count': len(group),
                'repositories': group['Repository'].unique().tolist(),
                'titles': group['Title'].unique().tolist(),
                'scripts': group['Script'].unique().tolist()
            })
    
    if duplicates:
        print(f"Found {len(duplicates)} global commands with duplicates:")
        for dup in duplicates:
            print(f"\nCommand: '{dup['command']}'")
            print(f"  Count: {dup['count']}")
            print(f"  Repositories: {dup['repositories']}")
            print(f"  Titles: {dup['titles']}")
            if len(set(dup['scripts'])) > 1:
                print(f"  Different Scripts: {dup['scripts']}")
    else:
        print("No duplicate global commands found.")
    
    return duplicates

def find_app_specific_duplicates(df):
    """Find duplicate commands within the same application"""
    print("\n" + "="*60)
    print("APPLICATION-SPECIFIC COMMAND DUPLICATES")
    print("="*60)
    
    # Group by Application, then by Command
    app_duplicates = []
    
    for app_name, app_group in df.groupby('Application'):
        if app_name == 'global':
            continue  # Skip global, already handled
            
        command_groups = app_group.groupby('Command')
        for command, group in command_groups:
            if len(group) > 1:
                app_duplicates.append({
                    'application': app_name,
                    'command': command,
                    'count': len(group),
                    'repositories': group['Repository'].unique().tolist(),
                    'titles': group['Title'].unique().tolist(),
                    'scripts': group['Script'].unique().tolist()
                })
    
    if app_duplicates:
        print(f"Found {len(app_duplicates)} application-specific command duplicates:")
        for dup in app_duplicates:
            print(f"\nApplication: '{dup['application']}'")
            print(f"Command: '{dup['command']}'")
            print(f"  Count: {dup['count']}")
            print(f"  Repositories: {dup['repositories']}")
            print(f"  Titles: {dup['titles']}")
            if len(set(dup['scripts'])) > 1:
                print(f"  Different Scripts: {dup['scripts']}")
    else:
        print("No application-specific command duplicates found.")
    
    return app_duplicates

def find_cross_app_command_duplicates(df):
    """Find the same voice command used across different applications"""
    print("\n" + "="*60)
    print("CROSS-APPLICATION COMMAND DUPLICATES")
    print("="*60)
    
    # Group by Command to see which applications use the same voice command
    command_groups = df.groupby('Command')
    cross_app_duplicates = []
    
    for command, group in command_groups:
        apps = group['Application'].unique()
        if len(apps) > 1:
            cross_app_duplicates.append({
                'command': command,
                'applications': apps.tolist(),
                'count': len(group),
                'repositories': group['Repository'].unique().tolist(),
                'titles': group['Title'].unique().tolist()
            })
    
    if cross_app_duplicates:
        print(f"Found {len(cross_app_duplicates)} commands used across multiple applications:")
        for dup in sorted(cross_app_duplicates, key=lambda x: x['count'], reverse=True)[:20]:  # Show top 20
            print(f"\nCommand: '{dup['command']}'")
            print(f"  Total occurrences: {dup['count']}")
            print(f"  Applications: {dup['applications']}")
            print(f"  Repositories: {dup['repositories']}")
    else:
        print("No cross-application command duplicates found.")
    
    return cross_app_duplicates

def find_repository_duplicates(df):
    """Find duplicate commands within the same repository but different applications"""
    print("\n" + "="*60)
    print("REPOSITORY-SPECIFIC COMMAND DUPLICATES")
    print("="*60)
    
    repo_duplicates = []
    
    for repo_name, repo_group in df.groupby('Repository'):
        command_groups = repo_group.groupby('Command')
        for command, group in command_groups:
            apps = group['Application'].unique()
            if len(apps) > 1:  # Same command in different apps within same repo
                repo_duplicates.append({
                    'repository': repo_name,
                    'command': command,
                    'applications': apps.tolist(),
                    'count': len(group),
                    'titles': group['Title'].unique().tolist()
                })
    
    if repo_duplicates:
        print(f"Found {len(repo_duplicates)} commands duplicated across applications within repositories:")
        for dup in repo_duplicates[:15]:  # Show first 15
            print(f"\nRepository: '{dup['repository']}'")
            print(f"Command: '{dup['command']}'")
            print(f"  Applications: {dup['applications']}")
            print(f"  Count: {dup['count']}")
    else:
        print("No repository-specific command duplicates found.")
    
    return repo_duplicates

def analyze_command_patterns(df):
    """Analyze common patterns and statistics"""
    print("\n" + "="*60)
    print("COMMAND ANALYSIS SUMMARY")
    print("="*60)
    
    total_commands = len(df)
    unique_commands = df['Command'].nunique()
    unique_apps = df['Application'].nunique()
    unique_repos = df['Repository'].nunique()
    
    print(f"Total commands: {total_commands}")
    print(f"Unique voice commands: {unique_commands}")
    print(f"Unique applications: {unique_apps}")
    print(f"Unique repositories: {unique_repos}")
    print(f"Duplicate rate: {((total_commands - unique_commands) / total_commands * 100):.1f}%")
    
    # Top applications by command count
    print(f"\nTop 10 applications by command count:")
    app_counts = df['Application'].value_counts().head(10)
    for app, count in app_counts.items():
        print(f"  {app}: {count} commands")
    
    # Top repositories by command count
    print(f"\nTop 10 repositories by command count:")
    repo_counts = df['Repository'].value_counts().head(10)
    for repo, count in repo_counts.items():
        print(f"  {repo}: {count} commands")

def main():
    csv_path = "VoiceLauncher/TalonCommands.csv"
    
    # Load the data
    df = load_talon_data(csv_path)
    if df is None:
        return
    
    # Clean the data - remove any rows with missing Command values
    df = df.dropna(subset=['Command'])
    
    # Run all analyses
    global_dups = find_global_command_duplicates(df)
    app_dups = find_app_specific_duplicates(df)
    cross_app_dups = find_cross_app_command_duplicates(df)
    repo_dups = find_repository_duplicates(df)
    analyze_command_patterns(df)
    
    # Summary
    print("\n" + "="*60)
    print("DUPLICATE ANALYSIS SUMMARY")
    print("="*60)
    print(f"Global command duplicates: {len(global_dups)}")
    print(f"App-specific duplicates: {len(app_dups)}")
    print(f"Cross-application duplicates: {len(cross_app_dups)}")
    print(f"Repository duplicates: {len(repo_dups)}")

if __name__ == "__main__":
    main()
